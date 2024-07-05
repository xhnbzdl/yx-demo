using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ProductSpikeCase.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IDatabase _redisDb;
        private static int count = 1;

        public OrderController(IOrderService orderService, IDatabase redisDb)
        {
            _orderService = orderService;
            _redisDb = redisDb;
        }

        /// <summary>
        /// 使用了redis库自带的LockTakeAsync获取锁
        /// </summary>
        /// <param name="quantity"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(int quantity)
        {
            var success = await _orderService.PlaceOrderAsync(1, quantity);
            if (success)
            {
                return Ok("Order placed successfully.");
            }
            else
            {
                return BadRequest("Failed to place order.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddStock(int stock)
        {
            await _orderService.AddStock(stock);
            return Ok("add stock successfully.");
        }

        /// <summary>
        /// 使用StringSet自实现的锁
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> AddOrder(string clientId,int qty=1)
        {
            string msg = null;
            //string clientId = Guid.NewGuid().ToString();
            // 设置锁
            var isSuccess = await SetLockVersionAsync(clientId);
            if (isSuccess)
            {
                try
                {
                    // 扣减库存
                    if (await SetInvQtyAsync(qty))
                    {
                        //Thread.Sleep(TimeSpan.FromSeconds(new Random().Next(1, 4)));
                        //Thread.Sleep(30000);
                        msg = $"执行次数{count++}，扣减成功,当前库存:{GetInvQty()}";
                    }
                    else
                    {
                        msg = $"执行次数{count++}，扣减失败,库存不足";
                    }
                }
                finally
                {
                    
                    await UnLockVersionAsync(clientId);
                }
            }
            else
            {
                msg = $"执行次数{count++}，资源正忙,请刷新后重试";
            }
            Console.WriteLine(msg);
            return msg;
        }

        private async Task<bool> SetLockVersionAsync(string value)
        {
            for (int i = 0; i < 10; i++)
            {
                var flag = await _redisDb.StringSetAsync("LockValue", value, TimeSpan.FromSeconds(3), When.NotExists, CommandFlags.None); //如果存在了返回false,不存在才返回true;
                if (flag)
                {
                    return true;
                }
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            return false;
        }

        private async Task<bool> UnLockVersionAsync(string value)
        {
            //var client = await _redisDb.StringGetAsync("LockValue");

            //if (client.IsNull || string.IsNullOrWhiteSpace(client))
            //{
            //    Console.WriteLine($"{value}的锁过期");
            //}

            //if (value.Equals(client))
            //{
            //    Console.WriteLine($"{value}释放了锁,锁值为{client}");
            //    return await _redisDb.KeyDeleteAsync("LockValue");
            //}

            //return false;
            var script = @"
            if redis.call('GET', KEYS[1]) == ARGV[1] then
                return redis.call('DEL', KEYS[1])
            else
                return 0
            end";
            var result = (int)await _redisDb.ScriptEvaluateAsync(script, new RedisKey[] { "LockValue" }, new RedisValue[] { value });
            if (result != 1)
            {
                Console.WriteLine($"{value}锁释放失败，锁过期");
            }
            return result == 1;
        }
        private int GetInvQty()
        {
            var qty = 0;
            qty = Convert.ToInt32(_redisDb.StringGet("InvQty"));
            return qty;
        }

        private async Task<bool> SetInvQtyAsync(int qty)
        {
            //使用脚本执行redis自减操作，当库存小于扣减库存时，则不进行自减
            var script = @"
                local currentQty = tonumber(redis.call('GET', KEYS[1]))
                local qtyToDecrement = tonumber(ARGV[1])
                if currentQty and currentQty >= qtyToDecrement then
                    redis.call('DECRBY', KEYS[1], qtyToDecrement)
                    return 1
                else
                    return 0
                end
            ";

            var result = (int)await _redisDb.ScriptEvaluateAsync(script, new RedisKey[] { "InvQty" }, new RedisValue[] { qty });
            return result == 1;
        }

    }
}
