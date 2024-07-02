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
        public async Task<string> AddOrder()
        {
            string msg = null;
            string clientId = Guid.NewGuid().ToString();
            var isSuccess = await SetLockVersionAsync(clientId);
            if (isSuccess)
            {
                try
                {
                    int invQty = GetInvQty();
                    if (invQty > 0)
                    {
                        invQty = invQty - 1;
                        SetInvQty(invQty);
                        //Thread.Sleep(30000);
                        msg = $"执行次数{count++}，扣减成功,当前库存:{invQty}";
                    }
                    else
                    {
                        msg = $"执行次数{count++}，扣减失败,库存不足";
                    }
                }
                finally
                {
                    //if (clientId.Equals(_redisDb.StringGet("LockValue")))
                    //{
                    //    await UnLockVersionAsync();  //释放锁;
                    //}
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
            var flag = await _redisDb.StringSetAsync("LockValue", value, TimeSpan.FromSeconds(10), When.NotExists, CommandFlags.None); //如果存在了返回false,不存在才返回true;
            return flag;
        }

        private async Task<bool> UnLockVersionAsync(string value)
        {

            //return await _redisDb.KeyDeleteAsync("LockValue");

            var script = @"
            if redis.call('GET', KEYS[1]) == ARGV[1] then
                return redis.call('DEL', KEYS[1])
            else
                return 0
            end";
            var result = (int)await _redisDb.ScriptEvaluateAsync(script, new RedisKey[] { "LockValue" }, new RedisValue[] { value });
            return result == 1;
        }
        private int GetInvQty()
        {
            var qty = 0;
            qty = Convert.ToInt32(_redisDb.StringGet("InvQty"));
            return qty;
        }

        private void SetInvQty(int qty)
        {
            _redisDb.StringSet("InvQty", qty);
        }

    }
}
