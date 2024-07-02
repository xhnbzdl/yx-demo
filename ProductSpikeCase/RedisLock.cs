using StackExchange.Redis;
using System.Threading;

namespace ProductSpikeCase
{
    public class RedisLock : IDisposable
    {
        private readonly IDatabase _db;
        private readonly string _lockKey;
        private readonly string _lockToken;

        public RedisLock(IDatabase db, string lockKey)
        {
            _db = db;
            _lockKey = lockKey;
            _lockToken = Guid.NewGuid().ToString();
        }
        /// <summary>
        /// 获取锁
        /// </summary>
        /// <returns></returns>
        public async Task<bool> GetLockAsync(TimeSpan timeout)
        {
            return await _db.LockTakeAsync(_lockKey, _lockToken, timeout);
        }

        /// <summary>
        /// 重试机制获取锁
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="retryCount"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public async Task<bool> TryGetLockAsync(int retryCount, TimeSpan timeout, TimeSpan delay)
        {
            for (int i = 0; i < retryCount; i++)
            {
                if (await _db.LockTakeAsync(_lockKey, _lockToken, timeout))
                {
                    return true;
                }
                await Task.Delay(delay);
            }
            return false;
        }

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <returns></returns>
        public async Task ReleaseLockAsync()
        {
            await _db.LockReleaseAsync(_lockKey, _lockToken);
        }

        public void Dispose()
        {
            ReleaseLockAsync().Wait();
        }
    }
}
