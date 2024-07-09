using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;

namespace PolicyDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Retry();
            //FallbackPolicy();
            //TimeOutPolicy();
            //CircuitBreakerPolicy();
            CombinedPolicyTest();
        }

        static void FallbackPolicy()
        {
            // 定义回退策略
            FallbackPolicy fallbackPolicy = Policy
                .Handle<DivideByZeroException>()
                .Or<Exception>()
                .Fallback(() =>
                {
                    Console.WriteLine("回退执行：默认结果为-1");
                },
                onFallback: (exception) =>
                {
                    Console.WriteLine($"回退触发，异常：{exception.Message}");
                });



            // 使用回退策略执行除法操作
            fallbackPolicy.Execute(() =>
            {
                Random random = new Random();
                int divisor = random.Next(0, 4); // 生成0到3之间的随机数

                Console.WriteLine($"尝试除法操作：10 / {divisor}");

                if (divisor == 0)
                {
                    throw new DivideByZeroException("除数不能为零");
                }
            });

            Console.WriteLine($"执行成功");
        }

        static void TimeOutPolicy()
        {
            // 定义超时策略，超时时间为5秒
            TimeoutPolicy timeoutPolicy = Policy
                .Timeout(TimeSpan.FromSeconds(5), TimeoutStrategy.Pessimistic,
                    onTimeout: (context, timespan, task) =>
                    {
                        Console.WriteLine($"操作超时：{timespan}");
                    });

            // 使用超时策略执行随机数测试
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    timeoutPolicy.Execute(() =>
                    {
                        Random random = new Random();
                        int sleepTime = random.Next(1, 10); // 生成1到9之间的随机秒数
                        Console.WriteLine($"任务开始{i}：将执行{sleepTime}秒");
                        Thread.Sleep(sleepTime * 1000); // 模拟任务执行时间
                    });
                }
                catch (TimeoutRejectedException ex)
                {
                    Console.WriteLine($"任务被超时策略中断：{ex.Message}");
                }
                finally
                {
                    Console.WriteLine($"任务完成{i}");
                }
            }
        }

        /// <summary>
        /// 断融
        /// </summary>
        static void CircuitBreakerPolicy()
        {
            // 定义断路器策略，在3次连续失败后打开断路器，持续1分钟
            CircuitBreakerPolicy circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreaker(3, TimeSpan.FromSeconds(10),
                    onBreak: (exception, duration) =>
                    {
                        Console.WriteLine($"断路器打开，持续时间：{duration}, 异常：{exception.Message}");
                    },
                    onReset: () => Console.WriteLine("断路器重置"),
                    onHalfOpen: () => Console.WriteLine("断路器半开"));


            // 使用断路器策略执行随机数测试
            Random random = new Random();
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    circuitBreakerPolicy.Execute(() =>
                    {
                        int number = random.Next(1, 5); // 生成1到4之间的随机数
                        Console.WriteLine($"生成的随机数：{number}");

                        if (number % 2 != 0) // 偶数模拟失败情况
                        {
                            throw new Exception($"随机数{number}除以2失败");
                        }

                        Console.WriteLine($"任务{i}成功完成");
                    });
                }
                catch (BrokenCircuitException ex)
                {
                    Console.WriteLine($"任务{i}被断路器拦截：{ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"任务{i}失败：{ex.Message}");
                }

                // 等待一段时间再进行下一次尝试
                Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// 异常重试
        /// </summary>

        static void Retry()
        {
            // 在捕获到Exception时，重试3次，每次延迟2s，等待时间成指数上升
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"重试 {retryCount} 次，等待时间： {timeSpan.TotalSeconds} 秒。异常信息： {exception.Message}");
                });

            try
            {
                retryPolicy.Execute(() =>
                {
                    var r = new Random().Next(1, 100);
                    if (r % 3 != 0)
                    {
                        throw new Exception($"随机数{r}无法整除以3");
                    }
                    Console.WriteLine($"随机数为{r}成功整除以3");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("重试3次后任然无法执行成功");
            }
        }

        static void CombinedPolicyTest()
        {
            // 定义重试策略，重试3次，每次延迟1秒
            RetryPolicy retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(1),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        Console.WriteLine($"重试次数：{retryCount}, 延迟时间：{timeSpan}, 异常：{exception.Message}");
                    });

            // 定义断路器策略，在3次连续失败后打开断路器，持续10秒
            CircuitBreakerPolicy circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreaker(3, TimeSpan.FromSeconds(10),
                    onBreak: (exception, duration) =>
                    {
                        Console.WriteLine($"断路器打开，持续时间：{duration}, 异常：{exception.Message}");
                    },
                    onReset: () => Console.WriteLine("断路器重置"),
                    onHalfOpen: () => Console.WriteLine("断路器半开"));

            // 定义超时策略，超时时间为2秒
            TimeoutPolicy timeoutPolicy = Policy
                .Timeout(TimeSpan.FromSeconds(2), TimeoutStrategy.Pessimistic,
                    onTimeout: (context, timespan, task) =>
                    {
                        Console.WriteLine($"操作超时：{timespan}");
                    });

            // 定义组合策略
            var combinedPolicy = Policy.Wrap(retryPolicy, circuitBreakerPolicy, timeoutPolicy);

            // 使用组合策略执行随机数测试
            Random random = new Random();
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    combinedPolicy.Execute(() =>
                    {
                        int number = random.Next(1, 10); // 生成1到9之间的随机数,只有3，5，7，9能成功
                        Console.WriteLine($"生成的随机数：{number}");

                        if (number <= 2) // 模拟超时情况
                        {
                            Console.WriteLine($"模拟任务{i}执行超过2秒");
                            Thread.Sleep(3000); // 模拟任务执行时间，超过2秒
                        }
                        else if (number % 2 == 0) // 偶数模拟失败情况
                        {
                            throw new Exception($"模拟任务{i}的随机失败");
                        }

                    });
                }
                catch (TimeoutRejectedException ex)
                {
                    Console.WriteLine($"任务{i}超时：{ex.Message}");
                }
                catch (BrokenCircuitException ex)
                {
                    Console.WriteLine($"任务{i}被断路器拦截：{ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"任务{i}失败：{ex.Message}");
                }
                finally
                {
                    Console.WriteLine($"任务{i}成功完成");
                }

                // 等待一段时间再进行下一次尝试
                Thread.Sleep(1000);


//定义重试策略：使用Polly的RetryPolicy，在捕获到异常时重试3次，每次延迟1秒。
//定义断路器策略：使用Polly的CircuitBreakerPolicy，在3次连续失败后打开断路器，断路器状态持续10秒。
//定义超时策略：使用Polly的TimeoutPolicy，设置操作超时时间为2秒。
//定义组合策略：将重试策略、断路器策略和超时策略组合在一起，形成一个组合策略。
//随机数生成和测试：在一个循环中生成1到9之间的随机数。如果随机数小于等于2，模拟任务超时；如果随机数是偶数，模拟任务失败；否则任务成功完成。
//输出结果：打印生成的随机数，任务成功完成或失败的信息，以及断路器和超时策略的状态变化的信息。
//等待时间：在每次任务尝试后等待1秒，以模拟任务的间隔时间。
//每次运行程序时，将会随机生成不同的任务状态，并测试组合策略是否生效。如果连续3次任务失败，断路器将打开，后续任务将被断路器拦截。
//在断路器打开10秒后，断路器将恢复正常状态，可以再次执行任务。同时，任务执行时间超过2秒时，超时策略将触发。
            }
        }
    }
}