namespace LockDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //BankAccount.Run();

            //MutexLock.Run();

            //SpinLockDemo.Run();

            //TestLock.Run();

            //ReadWirteLock.Run();

            RedisLock.RunAsync().Wait();


        }
    }
}