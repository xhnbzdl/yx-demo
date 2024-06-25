using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Dashboard.BasicAuthorization;

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

            #region 中间件，用于 Basic Authentication,放在swagger和hangfire前面可以限制访问两者都需要校验账户密码
            //app.Use(async (context, next) =>
            //{
            //    var authHeader = context.Request.Headers["Authorization"].ToString();
            //    if (authHeader != null && authHeader.StartsWith("Basic"))
            //    {
            //        var encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
            //        var decodedUsernamePassword = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));
            //        var username = decodedUsernamePassword.Split(':')[0];
            //        var password = decodedUsernamePassword.Split(':')[1];

            //        if (username == "admin" && password == "admin")
            //        {
            //            await next.Invoke();
            //        }
            //        else
            //        {
            //            context.Response.Headers["WWW-Authenticate"] = "Basic";
            //            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            //        }
            //    }
            //    else
            //    {
            //        // 浏览器弹框输入账户密码
            //        context.Response.Headers["WWW-Authenticate"] = "Basic";
            //        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            //    }
            //});
            #endregion

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
            //配置服务最大重试次数值
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 5 });

            //启用Hangfire仪表盘和服务器(支持使用Hangfire而不是默认的后台作业管理器)
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                // hangfire权限默认使用LocalRequestsOnlyAuthorizationFilter，只支持部署hangfire的服务器访问dashboard，只有设置为空数组才可让其他ip访问
                //Authorization = new IDashboardAuthorizationFilter[] { },

                 使用账户密码登录面板
                Authorization = new IDashboardAuthorizationFilter[] {
                    new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
                    {
                        RequireSsl = false,
                        SslRedirect = false,
                        LoginCaseSensitive = true,
                        Users = new []
                        {
                            new BasicAuthAuthorizationUser
                            {
                                Login = "admin",
                                PasswordClear =  "admin"
                            }
                        }
                    })
                }
            });
        }
    }
}