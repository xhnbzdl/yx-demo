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
