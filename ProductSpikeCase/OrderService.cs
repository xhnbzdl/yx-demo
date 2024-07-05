using StackExchange.Redis;
using System;

namespace ProductSpikeCase
{
    public interface IOrderService
    {
        /// <summary>
        /// 下订单
        /// </summary>
        /// <returns></returns>
        Task<bool> PlaceOrderAsync(int productId, int quantity);

        Task AddStock(int stock);
    }

    public class OrderService:IOrderService
    {
        private readonly IDatabase _redisDb;

        public OrderService(IDatabase redisDb)
        {
            _redisDb = redisDb;
        }

        public async Task AddStock(int stock)
        {
            await _redisDb.StringSetAsync("iphone",stock);
        }

        /// <summary>
        /// 下订单
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public async Task<bool> PlaceOrderAsync(int productId, int quantity)
        {
            var lockKey = $"lock:product:{productId}";
            using (var redisLock = new RedisLock(_redisDb, lockKey))
            {
                if (await redisLock.TryGetLockAsync(10,TimeSpan.FromSeconds(30),TimeSpan.FromSeconds(1)))
                {
                    try
                    {
                        Thread.Sleep(30000);
                        if (await SetInvQtyAsync(quantity))
                        {
                            var productStock = (int)await _redisDb.StringGetAsync("iphone");
                            Console.WriteLine($"success:下单成功，数量{quantity}，库存剩余{productStock}");
                            return true;
                        }
                        Console.WriteLine("error:库存不足");
                        return false; // 库存不足
                    }
                    finally
                    {
                        await redisLock.ReleaseLockAsync();
                    }
                }
                else
                {
                    Console.WriteLine("warn:请求繁忙稍后再试");
                    return false; // 无法获取锁
                }
            }
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

            var result = (int)await _redisDb.ScriptEvaluateAsync(script, new RedisKey[] { "iphone" }, new RedisValue[] { qty });
            return result == 1;
        }
    }
}
