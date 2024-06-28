using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockDemo
{
    public class ReadWirteLock
    {
        private static ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
        private static int sharedResource = 0;
        private static string filePath = "sharedFile.txt";

        public static void Run()
        {
            File.WriteAllText(filePath, string.Empty);
            Task[] readers = new Task[5];
            Task[] writers = new Task[2];

            // 创建多个读任务
            for (int i = 0; i < readers.Length; i++)
            {
                readers[i] = Task.Run(() => Read());
            }

            // 创建多个写任务
            for (int i = 0; i < writers.Length; i++)
            {
                writers[i] = Task.Run(() => Write());
            }

            Task.WaitAll(readers);
            Task.WaitAll(writers);

            Console.WriteLine("所有任务完成。");
        }

        static void Read()
        {
            for (int i = 0; i < 10; i++)
            {
                rwLock.EnterReadLock();
                try
                {
                    string content = File.ReadAllText(filePath);
                    //Console.WriteLine($"读取线程 {Thread.CurrentThread.ManagedThreadId} 读取 sharedResource: {sharedResource}");
                    Console.WriteLine($"读取线程 {Thread.CurrentThread.ManagedThreadId} 读取内容: {content}");
                }
                finally
                {
                    rwLock.ExitReadLock();
                }

                Thread.Sleep(100); // 模拟读取操作的开销
            }
        }

        static void Write()
        {
            for (int i = 0; i < 5; i++)
            {
                //rwLock.EnterWriteLock();
                try
                {
                    //sharedResource++;
                    //Console.WriteLine($"写入线程 {Thread.CurrentThread.ManagedThreadId} 更新 sharedResource 为: {sharedResource}");
                    string content = $"写入线程 {Thread.CurrentThread.ManagedThreadId} 在 {DateTime.Now}\n";
                    File.AppendAllText(filePath, content);
                    Console.WriteLine($"写入线程 {Thread.CurrentThread.ManagedThreadId} 写入内容: {content}");
                }
                finally
                {
                    //rwLock.ExitWriteLock();
                }

                Thread.Sleep(200); // 模拟写入操作的开销
            }
        }
    }
}
