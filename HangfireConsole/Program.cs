using Hangfire;
using Hangfire.MemoryStorage;

namespace HangfireConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {

            // 配置Hangfire使用内存存储
            //GlobalConfiguration.Configuration.UseMemoryStorage();
            GlobalConfiguration.Configuration.UseSqlServerStorage("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=YxDemoDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");

            HangfireTasks hangfire = new HangfireTasks();
            //hangfire.SimpleTasks();
            //hangfire.RetryTasks();
            //hangfire.DependentTasks();
            hangfire.ConfigIntervalTasks();
        }
    }

}