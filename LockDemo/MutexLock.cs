using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockDemo
{
    public class MutexLock
    {
        public static void Run()
        {
            bool createdNew;
            Mutex mutex = new Mutex(false, "Global\\MyMutex", out createdNew);

            if (createdNew)
            {
                Console.WriteLine("创建了一个新的互斥锁。");
            }
            else
            {
                Console.WriteLine("使用了现有的互斥锁。");
            }

            Console.WriteLine("等待互斥锁...");
            mutex.WaitOne();

            Console.WriteLine("进入临界区。");
            Console.WriteLine("按任意键释放互斥锁并退出。");
            Console.ReadKey();

            mutex.ReleaseMutex();
        }
        // 测试mutex需要起两个进程来测试，复制bin目录启动两个控制台可测试
    }
}
