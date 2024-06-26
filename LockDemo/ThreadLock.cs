using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockDemo
{
    /// <summary>
    /// 银行账户的并发操作，互斥锁
    /// </summary>
    public class BankAccount
    {
        /// <summary>
        /// 账户余额
        /// </summary>
        private decimal balance;
        /// <summary>
        /// 对象用于锁定代码块，确保同时只有一个线程能访问和修改 
        /// </summary>
        private readonly object balanceLock = new object();

        public BankAccount(decimal initialBalance)
        {
            balance = initialBalance;
        }

        public static void Run()
        {
            // 创建一个初始余额为 1000 的 BankAccount 对象。
            BankAccount account = new BankAccount(1000m);
            // 创建 10 个线程，偶数线程执行存款操作，奇数线程执行取款操作
            Thread[] threads = new Thread[10];

            for (int i = 0; i < threads.Length; i++)
            {
                if (i % 2 == 0)
                {
                    threads[i] = new Thread(() => account.Deposit(100m))
                    {
                        Name = $"Thread {i}"
                    };
                }
                else
                {
                    threads[i] = new Thread(() => account.Withdraw(50m))
                    {
                        Name = $"Thread {i}"
                    };
                }
            }

            foreach (Thread t in threads)
            {
                t.Start();
            }

            foreach (Thread t in threads)
            {
                t.Join();
            }
            // 输出最终余额
            Console.WriteLine($"最终余额: {account.GetBalance():C}");
        }
        /// <summary>
        /// 增加余额，使用 lock 确保线程安全。
        /// </summary>
        /// <param name="amount"></param>
        public void Deposit(decimal amount)
        {
            lock (balanceLock)
            {
                Console.WriteLine($"{Thread.CurrentThread.Name} 正在存入 {amount:C}");
                balance += amount;
                Console.WriteLine($"{Thread.CurrentThread.Name} 新的余额: {balance:C}");
            }
        }
        /// <summary>
        /// 方法减少余额，使用 lock 确保线程安全
        /// </summary>
        /// <param name="amount"></param>
        public void Withdraw(decimal amount)
        {
            lock (balanceLock)
            {
                if (balance >= amount)
                {
                    Console.WriteLine($"{Thread.CurrentThread.Name} 正在取出 {amount:C}");
                    balance -= amount;
                    Console.WriteLine($"{Thread.CurrentThread.Name} 新的余额: {balance:C}");
                }
                else
                {
                    Console.WriteLine($"{Thread.CurrentThread.Name} 试图提取 {amount:C}, 但资金不足.");
                }
            }
        }
        /// <summary>
        /// 方法返回当前余额，也使用 lock 确保线程安全
        /// </summary>
        /// <returns></returns>
        public decimal GetBalance()
        {
            lock (balanceLock)
            {
                return balance;
            }
        }

        //不加锁可能的问题
        //    1. 数据竞争
        //        由于多个线程可能同时访问和修改 balance 字段，以下问题可能发生：
        //        存款和取款操作的交错：一个线程可能读取了旧的余额，而另一个线程在此期间修改了余额。
        //        丢失更新：两个线程可能同时读取相同的余额值，分别进行操作并写回结果，导致一个更新被覆盖。
        //    2. 部分操作丢失
        //        由于线程的上下文切换，以下问题可能发生：
        //        未能正确更新余额：多个线程可能在同一时刻读取余额值，并基于旧值进行修改，最终只保存一个更新，其他更新丢失。
    }
}
