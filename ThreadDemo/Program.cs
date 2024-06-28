namespace ThreadDemo
{
    internal class Program
    {
        static void Main()
        {
            Console.WriteLine($"主线程ID:{Thread.CurrentThread.ManagedThreadId},开始执行");
            //ThreadDoWork();
            //ThreadPoolDoWork();
            TaskDoWork();
            //Sum();

            Console.WriteLine($"主线程ID:{Thread.CurrentThread.ManagedThreadId},执行完毕");
            Console.ReadKey();
        }

        static void ThreadDoWork()
        {
            var action = () =>
            {
                Console.WriteLine("子线程开始执行。");
                Thread.Sleep(1000); // 模拟执行1秒钟
                Console.WriteLine("子线程执行完成。");
            };

            // 创建一个新线程
            Thread thread = new Thread(new ThreadStart(action));
            // 启动线程执行
            thread.Start();
            // 等待子线程执行完成，注释该行则多线程并行
            thread.Join();

            Console.WriteLine("主线程结束。");
        }

        static void ThreadPoolDoWork()
        {
            // 向线程池添加工作项
            ThreadPool.QueueUserWorkItem(DoWork, "工作项1");
            ThreadPool.QueueUserWorkItem(DoWork, "工作项2");
            Console.WriteLine("主线程结束。");
        }

        static void TaskDoWork()
        {
            // Task.Run 不总是启动新线程，它会将工作项调度到任务调度器，默认情况下，任务调度器会使用线程池中的线程来执行任务。
            // 线程池会根据负载动态分配线程，因此 Task.Run 可能会重用现有的线程池线程，而不是每次都创建新的线程
            for (int i = 0; i < 5; i++)
            {
                Task.Run(() => DoWork());
            }
        }

        static void Sum()
        {
            int[] data = Enumerable.Range(0, 10000000).ToArray();

            // 使用并行计算求和
            long sum = 0;
            Parallel.ForEach(data, x => { Interlocked.Add(ref sum, x); });

            Console.WriteLine("和为：" + sum);
        }

        static void DoWork(object state)
        {
            Console.WriteLine("工作项开始执行：" + state);
            Thread.Sleep(1000); // 模拟执行1秒钟
            Console.WriteLine("工作项执行完成：" + state);
        }

        static void DoWork()
        {
            Console.WriteLine($"线程{Thread.CurrentThread.ManagedThreadId}异步操作开始执行。");
            Task.Delay(1000).Wait(); // 模拟执行1秒钟
            Console.WriteLine($"线程{Thread.CurrentThread.ManagedThreadId}异步操作执行完成。");
        }
    }
}