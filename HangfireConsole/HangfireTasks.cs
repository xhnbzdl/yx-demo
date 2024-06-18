using Hangfire;
using Hangfire.MemoryStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HangfireConsole
{
    /// <summary>
    /// 存储于内存中
    /// </summary>
    public class HangfireTasks
    {
        /// <summary>
        /// 简单的任务
        /// </summary>
        public void SimpleTasks()
        {
            // 创建并启动Hangfire服务器
            using (var server = new BackgroundJobServer())
            {
                Console.WriteLine("Hangfire 服务器启动时间{0}。按任意键退出...", DateTime.Now);

                // 调度一个简单的任务
                BackgroundJob.Enqueue(() => Console.WriteLine("Hello, Hangfire!"));

                // 调度一个延时任务，默认延时为15秒，配置3秒也无效
                BackgroundJob.Schedule(() => Console.WriteLine("该信息延迟 3 秒"), TimeSpan.FromSeconds(3));

                // 调度一个循环任务,每分钟一次，默认为15秒一次
                RecurringJob.AddOrUpdate("recurring-job", () => Console.WriteLine("执行重复性工作,一分钟一次"), Cron.Minutely);

                // 防止程序立即退出
                Console.ReadKey();

            }
        }
        /// <summary>
        /// 任务重试和错误处理
        /// </summary>

        public void RetryTasks()
        {
            using (var server = new BackgroundJobServer())
            {
                Console.WriteLine("Hangfire 服务器启动时间{0}。按任意键退出...", DateTime.Now);

                // 调度一个任务，模拟失败后自动重试
                BackgroundJob.Enqueue(() => PerformTaskWithRetry());

                // 防止程序立即退出
                Console.ReadKey();
            }
        }

        /// <summary>
        /// 依赖任务
        /// </summary>
        public void DependentTasks()
        {
            using (var server = new BackgroundJobServer())
            {
                Console.WriteLine("Hangfire 服务器启动时间{0}。按任意键退出...",DateTime.Now);

                // 调度一个延时任务，默认延时为15秒，配置3秒也无效
                var jobId = BackgroundJob.Schedule(() => LatencyConsole(), TimeSpan.FromSeconds(3));
                // 依赖任务1
                BackgroundJob.ContinueJobWith(jobId, () => Console.WriteLine("任务 2：从属任务"));

                // 防止程序立即退出
                Console.ReadKey();
            }
        }
        /// <summary>
        /// 配置Hangfire默认间隔时间
        /// </summary>

        public void ConfigIntervalTasks()
        {
            var options = new BackgroundJobServerOptions
            {
                SchedulePollingInterval = TimeSpan.FromSeconds(1),
            };

            using (var server = new BackgroundJobServer(options))
            {
                Console.WriteLine("Hangfire 服务器启动时间{0}。按任意键退出...", DateTime.Now);

                // 调度一个延时任务
                Console.WriteLine("延迟3秒的任务");
                BackgroundJob.Schedule(() => ConsoleWrite(), TimeSpan.FromSeconds(3));
                Thread.Sleep(3500);
                Console.WriteLine("每秒执行一次的任务");
                RecurringJob.AddOrUpdate("recurring-job",()=> ConsoleWrite(),"* * * * * *");

                // 防止程序立即退出
                Console.ReadKey();
            }
        }

        public static void ConsoleWrite()
        {
            Console.WriteLine("当前时间：{0}", DateTime.Now);

        }

        /// <summary>
        /// 延迟打印
        /// </summary>
        public static void LatencyConsole()
        {
            Console.WriteLine("任务 1：初始任务：该信息延迟 15 秒");
            Console.WriteLine("当前时间：{0}",DateTime.Now);
        }

        /// <summary>
        /// 执行带重试的任务，最多重试5次
        /// </summary>
        /// <exception cref="Exception"></exception>
        [AutomaticRetry(Attempts = 5, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public static void PerformTaskWithRetry()
        {
            Console.WriteLine("Performing task with retry...");
            throw new Exception("Simulated task failure");
        }
    }
}
