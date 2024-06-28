using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LockDemo
{
    /// <summary>
    /// 自旋锁和普通互斥锁lock的比较
    /// </summary>
    public class TestLock
    {

        private static readonly object lockObject = new object();
        private static SpinLock spinLock = new SpinLock();
        private static int sharedResource = 0;
        // 设置为10，sleep 1 可测试出当线程执行时间长时，lock耗时比spinlock快
        // 设置为1000000时，不sleep 可测试出spinlock耗时比lock快
        private static int iterations = 1000000;

        public static void Run()
        {
            Console.WriteLine("测试开始...");

            // 测试 lock 的性能
            Console.WriteLine("Testing lock...");
            var lockTime = TestLockPerformance();
            Console.WriteLine($"lock 时间: {lockTime.TotalMilliseconds} 毫秒");

            // 测试 SpinLock 的性能
            Console.WriteLine("Testing SpinLock...");
            var spinLockTime = TestSpinLockPerformance();
            Console.WriteLine($"SpinLock 时间: {spinLockTime.TotalMilliseconds} 毫秒");

            Console.WriteLine("测试结束。");
        }

        static TimeSpan TestLockPerformance()
        {
            sharedResource = 0;
            var stopwatch = Stopwatch.StartNew();

            Thread[] threads = new Thread[Environment.ProcessorCount];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() =>
                {
                    for (int j = 0; j < iterations; j++)
                    {
                        lock (lockObject)
                        {
                            sharedResource++;
                        }
                    }
                });
            }

            foreach (var thread in threads) thread.Start();
            foreach (var thread in threads) thread.Join();

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        static TimeSpan TestSpinLockPerformance()
        {
            sharedResource = 0;
            var stopwatch = Stopwatch.StartNew();

            Thread[] threads = new Thread[Environment.ProcessorCount];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() =>
                {
                    for (int j = 0; j < iterations; j++)
                    {
                        bool lockTaken = false;
                        try
                        {
                            spinLock.Enter(ref lockTaken);
                            sharedResource++;
                            //Thread.Sleep(1);
                        }
                        finally
                        {
                            if (lockTaken) spinLock.Exit();
                        }
                    }
                });
            }

            foreach (var thread in threads) thread.Start();
            foreach (var thread in threads) thread.Join();

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
        // 自旋锁的实现机制：自旋等待，即在获取锁的过程中会不断轮询锁状态，直到成功获取锁或放弃。
        // 锁持有时间短：当锁持有时间非常短时，SpinLock 可能表现更好，因为它避免了线程阻塞和唤醒的上下文切换开销。
        // 锁持有时间长：当锁持有时间较长时，SpinLock 可能表现更差，因为自旋锁浪费了大量的 CPU 资源，导致整体性能下降。
        // lock 适用于大多数场景，特别是当锁持有时间较长时，它的线程阻塞机制可以更有效地利用系统资源。
        // SpinLock 适用于锁持有时间非常短的场景，可以避免线程上下文切换的开销，提高性能。
    }
}
