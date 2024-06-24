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
                    // �����������õ������������������ʱʱ��
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    // ���ú����ĳ�ʱʱ�䣬ȷ���������ڼ䲻�ᱻ������������ռ��
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    // ��ҵ������ѯ�����Ĭ��ֵΪ15�룬��Ҫ���Ƽ�ʱ����
                    // ��ͬһʱ����ж�������Ҵ���ʱ�������ѯʱ��ʱ�����ֳ���
                    QueuePollInterval = TimeSpan.FromSeconds(5),
                    // ���� Hangfire ʹ���Ƽ���������뼶��
                    UseRecommendedIsolationLevel = true,
                    // ����ȫ������������� SQL Server �洢���Ƽ������ã�����ȫ��������������������
                    DisableGlobalLocks = true,
                });
            });

            if (configraution.GetValue<bool>("HangfireServerEnable"))
            {
                builder.Services.AddHangfireServer(options =>
                {
                    // ���������
                    options.ServerName = $"localhost:{DateTime.Now}";
                    // ��Ҫ������ʱ����ͼƻ�����
                    // ʱ����Խ�̣�ɨ��ƻ���ҵ���е�Ƶ��Խ�ߣ����Ը���ط��ֹ�������ִ�У���Ҳ�����Ӻ�̨�������ĸ�������Դռ�á�
                    // ʱ����Խ��������ζ��ɨ��ƻ���ҵ���е�Ƶ��Խ�ͣ�ϵͳ��������Դռ��Ҳ��Ӧ���٣������ܻᵼ�¹��������޷���ʱִ�С�
                    // ֵ�����ҪС�ڼƻ����������ʱ�䣬�����������������ʧ
                    options.SchedulePollingInterval = TimeSpan.FromSeconds(configraution.GetValue<int>("SchedulePollingInterval"));
                    // ָ���� Hangfire �������ڽ��յ��ر��źź󣬵ȴ�����ִ�е�������ɵ�ʱ�䡣
                    options.ShutdownTimeout = TimeSpan.FromMinutes(5);
                    // ����������������������
                    // Queues ��������������ҵ������䵽��ͬ�Ķ����У��Ա�ʹ�ò�ͬ�Ĵ�������������Щ���У��Ӷ�ʵ����ҵ��������ȼ��ͷ������
                    options.Queues = configraution["HangfireQueues"].Split(',');
                    // ���÷��������Բ���������������
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
        /// ����Hangfire
        /// </summary>
        /// <param name="app"></param>
        protected static void UseHangfire(IApplicationBuilder app)
        {
            // TODO: �ж��Ƿ����� HangfireDashboard

            //���÷���������Դ���ֵ
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 5 });
            //����Hangfire�����ѡ����
            var jobOptions = new BackgroundJobServerOptions
            {
                //wait all jobs performed when BackgroundJobServer shutdown.
                ShutdownTimeout = TimeSpan.FromMinutes(30),
                Queues = new[] { "default", "jobs" }, //�������ƣ�ֻ��ΪСд
                WorkerCount = 3, //Environment.ProcessorCount * 5, //���������� Math.Max(Environment.ProcessorCount, 20)
                ServerName = "GCT.AbpExsoft.hangfire",
            };

            //����Hangfire�Ǳ��̺ͷ�����(֧��ʹ��Hangfire������Ĭ�ϵĺ�̨��ҵ������)
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {

            });
        }
    }
}