using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace HangfireWebApp.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class HangfrieTestController : ControllerBase
    {

        /// <summary>
        /// ��ʱ����
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public void InstantTasks(string queueName = "job1")
        {
            Console.WriteLine($"����ӿڵ�ʱ��{DateTime.Now}");
            BackgroundJob.Enqueue<ITestService>(queueName, x => x.InstantTasks(DateTime.Now.ToString()));
        }

        [HttpGet]
        public void DelayedTask()
        {
            BackgroundJob.Schedule<ITestService>(x => x.DelayedTask(DateTime.Now.ToString()),TimeSpan.FromSeconds(1));
        }

        [HttpGet]
        public void PeriodicityTask()
        {
            RecurringJob.AddOrUpdate<ITestService>("PeriodicityTask","job1", x => x.PeriodicityTask(DateTime.Now.ToString()), "* * * * * *");
        }
    }
}