using Hangfire;

namespace HangfireWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configraution = builder.Configuration;
            // Add services to the container.
            builder.Services.AddTransient<ITestService, TestService>();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddHangfire(config =>
            {
                config.UseSqlServerStorage("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=YxDemodb;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False",new Hangfire.SqlServer.SqlServerStorageOptions
                {
                    // 参数用于配置单个命令批处理的最大超时时间
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    // 设置合理的超时时间，确保任务处理期间不会被其他服务器抢占。
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    // 作业队列轮询间隔。默认值为15秒，主要控制即时任务
                    // 当同一时间段有多个任务，且处理时间大于轮询时间时可体现出来
                    QueuePollInterval = TimeSpan.FromSeconds(5),
                    // 设置 Hangfire 使用推荐的事务隔离级别。
                    UseRecommendedIsolationLevel = true,
                    // 禁用全局锁定，这对于 SQL Server 存储是推荐的配置，避免全局锁定带来的性能问题
                    DisableGlobalLocks = true,
                });
            });

            if (configraution.GetValue<bool>("HangfireServerEnable"))
            {
                builder.Services.AddHangfireServer(options =>
                {
                    // 服务端名称
                    options.ServerName = $"localhost:{DateTime.Now}";
                    // 主要控制延时任务和计划任务
                    // 时间间隔越短，扫描计划作业队列的频率越高，可以更快地发现过期任务并执行，但也会增加后台处理器的负担和资源占用。
                    // 时间间隔越长，则意味着扫描计划作业队列的频率越低，系统负担和资源占用也相应减少，但可能会导致过期任务无法及时执行。
                    // 值最务必要小于计划任务的周期时间，如果大于则会出现任务丢失
                    options.SchedulePollingInterval = TimeSpan.FromSeconds(configraution.GetValue<int>("SchedulePollingInterval"));
                    // 指定了 Hangfire 服务器在接收到关闭信号后，等待正在执行的任务完成的时间。
                    options.ShutdownTimeout = TimeSpan.FromMinutes(5);
                    // 定义服务器处理的任务队列
                    // Queues 参数允许您将作业任务分配到不同的队列中，以便使用不同的处理器来处理这些队列，从而实现作业任务的优先级和分组管理
                    options.Queues = configraution["HangfireQueues"].Split(',');
                    // 配置服务器可以并发处理的任务数
                    options.WorkerCount = 2;
                });
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            UseHangfire(app);

            app.MapControllers();

            app.Run();
        }

        /// <summary>
        /// 启用Hangfire
        /// </summary>
        /// <param name="app"></param>
        protected static void UseHangfire(IApplicationBuilder app)
        {
            // TODO: 判断是否启用 HangfireDashboard

            //配置服务最大重试次数值
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 5 });
            //设置Hangfire服务可选参数
            var jobOptions = new BackgroundJobServerOptions
            {
                //wait all jobs performed when BackgroundJobServer shutdown.
                ShutdownTimeout = TimeSpan.FromMinutes(30),
                Queues = new[] { "default", "jobs" }, //队列名称，只能为小写
                WorkerCount = 3, //Environment.ProcessorCount * 5, //并发任务数 Math.Max(Environment.ProcessorCount, 20)
                ServerName = "GCT.AbpExsoft.hangfire",
            };

            //启用Hangfire仪表盘和服务器(支持使用Hangfire而不是默认的后台作业管理器)
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {

            });
        }
    }
}