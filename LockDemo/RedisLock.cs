using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockDemo
{
    /// <summary>
    /// 分布式锁
    /// </summary>
    public class RedisLock
    {
        private static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("192.168.2.61,password=bb123456");
        private static IDatabase db = redis.GetDatabase(1);
        private static readonly string lockKey = "my_distributed_lock";

        public static async Task RunAsync()
        {
            Task[] tasks = new Task[5];

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() => Process());
            }

            await Task.WhenAll(tasks);

            Console.WriteLine("所有任务完成。");

            /// 运行结果：你会看到多个线程尝试获取锁，并且只有获取锁的线程可以进行任务处理，其他线程会等待下一次尝试。
            /// 注意事项：锁的过期时间：要设置一个合理的过期时间来防止死锁，但也不能过短以防任务未完成锁就过期。
            ///         锁的唯一性：每个获取锁的线程应该有唯一的标识来防止误释放别的线程获取的锁。
            ///         RedLock算法：对于更高的可靠性，可以实现RedLock算法，这里只是简单的分布式锁实现。
        }

        /// <summary>
        /// 模拟一个任务流程，尝试获取锁，处理任务，释放锁
        /// </summary>
        /// <returns></returns>
        static async Task Process()
        {
            for (int i = 0; i < 5; i++)
            {
                if (await AcquireLockAsync(lockKey, TimeSpan.FromMinutes(1)))
                {
                    try
                    {
                        Console.WriteLine($"线程 {Thread.CurrentThread.ManagedThreadId} 获取锁并开始处理...");
                        await Task.Delay(6000); // 模拟处理时间
                        Console.WriteLine($"线程 {Thread.CurrentThread.ManagedThreadId} 处理完成。");
                    }
                    finally
                    { 
                        await ReleaseLockAsync(lockKey);
                        Console.WriteLine($"线程 {Thread.CurrentThread.ManagedThreadId} 释放锁。");
                    }
                }
                else
                {
                    Console.WriteLine($"线程 {Thread.CurrentThread.ManagedThreadId} 未能获取锁。");
                    i--;
                }

                await Task.Delay(500); // 模拟其他操作的时间
            }
        }

        /// <summary>
        /// 尝试获取锁，使用SETNX命令，如果获取成功则设置一个过期时间来防止死锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        static async Task<bool> AcquireLockAsync(string key, TimeSpan expiry)
        {
            var token = Guid.NewGuid().ToString();
            // When.NotExists 的含义是只有当键不存在时，才会设置键值对。这在实现分布式锁时非常重要，因为我们只希望在锁尚未被其他客户端持有的情况下，才能成功获取锁
            // 设置成功返回true，设置不成功返回false，当设置不成功则代表锁正在被使用
            return await db.StringSetAsync(key, token, expiry, When.NotExists);
        }

        /// <summary>
        /// 释放锁，删除Redis中的锁键
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        static async Task ReleaseLockAsync(string key)
        {
            await db.KeyDeleteAsync(key);
        }
    }
}
