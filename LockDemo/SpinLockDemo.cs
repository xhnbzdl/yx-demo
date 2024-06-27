using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockDemo
{
    public class SpinLockDemo
    {
        // 创建一个 SpinLock 实例
        private static SpinLock spinLock = new SpinLock();
        static void AccessResource()
        {
            bool lockTaken = false;
            try
            {
                // 尝试获取自旋锁
                spinLock.Enter(ref lockTaken);

                // 执行临界区代码
                Console.WriteLine($"线程 {Thread.CurrentThread.Name} 进入临界区，开始执行...");
                Thread.Sleep(2000); // 模拟一些工作
                Console.WriteLine($"线程 {Thread.CurrentThread.Name} 完成执行，即将离开临界区。");
            }
            finally
            {
                // 释放自旋锁
                if (lockTaken)
                {
                    spinLock.Exit();
                }
            }
        }

        public static void Run()
        {
            // 启动多个线程来演示自旋锁的效果
            for (int i = 1; i <= 5; i++)
            {
                Thread thread = new Thread(() =>
                {
                    AccessResource();
                });
                thread.Name = $"Thread-{i}";
                thread.Start();
            }

            Console.ReadLine();
        }
    }
}
