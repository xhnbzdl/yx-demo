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
                if (await redisLock.GetLockAsync(TimeSpan.FromSeconds(30)))
                {
                    try
                    {
                        var productStock = (int) await _redisDb.StringGetAsync("iphone");
                        if (productStock < quantity)
                        {
                            return false; // 库存不足
                        }

                        productStock -= quantity;

                        await _redisDb.StringSetAsync("iphone",productStock.ToString());
                        return true;
                    }
                    finally
                    {
                        await redisLock.ReleaseLockAsync();
                    }
                }
                else
                {
                    return false; // 无法获取锁
                }
            }
        }
    }
}
