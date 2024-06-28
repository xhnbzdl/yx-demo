namespace LockDemo
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //BankAccount.Run();

            //MutexLock.Run();

            //SpinLockDemo.Run();

            //TestLock.Run();

            //ReadWirteLock.Run();

            await RedisLock.RunAsync();


        }
    }
}