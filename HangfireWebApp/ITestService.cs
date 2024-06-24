namespace HangfireWebApp
{
    public interface ITestService
    {
        void InstantTasks(string startTime);

        void DelayedTask(string startTime);

        void PeriodicityTask(string startTime);
    }

    public class TestService : ITestService
    {
        public void InstantTasks(string startTime)
        {
            Thread.Sleep(10000);// 为了测试QueuePollInterval值的效果
            Console.WriteLine($"即时任务的触发时间：{startTime}，任务的实际执行时间：{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
        }

        public void DelayedTask(string startTime)
        {
            Console.WriteLine($"延时任务的触发时间：{startTime}，任务的实际执行时间：{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
        }

        public void PeriodicityTask(string startTime)
        {
            Console.WriteLine($"定时任务的触发时间：{startTime}，任务的实际执行时间：{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
        }
    }
}
