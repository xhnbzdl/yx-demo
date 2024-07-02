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
                        var productStock = (int) await _redisDb.StringGetAsync("iphone");
                        if (productStock < quantity)
                        {
                            Console.WriteLine("error:库存不足");
                            return false; // 库存不足
                        }
                        productStock -= quantity;
                        Console.WriteLine($"success:下单成功，数量{quantity}，库存剩余{productStock}");
                        await _redisDb.StringSetAsync("iphone",productStock);
                        return true;
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
    }
}
