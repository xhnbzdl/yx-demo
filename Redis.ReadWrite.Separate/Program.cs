using CSRedis;
using FreeRedis;
using StackExchange.Redis;

namespace Redis.ReadWrite.Separate
{
    internal class Program
    {
        static void Main(string[] args)
        {
            HaproxyTest();
        }

        /// <summary>
        /// CSRedisCore的哨兵模式只是实现了高可用，并没有自带读写分离，读写均走主库，也就是说它只是能通过哨兵自动实现切换主库链接，
        /// 如果使用这种方式就不需要部署haproxy来统一入口了。
        /// https://github.com/2881099/csredis/issues/238中提到CSRedisCore不会支持读写分离了。
        /// </summary>
        static void CSRedisSentinelTest()
        {
            // 哨兵的地址
            var sentinelAddresses = "192.168.2.130:26379,192.168.2.148:26379,192.168.2.149:26379";
            //连接哨兵
            var csredis = new CSRedisClient("mymaster,password=bb123456,defaultDatabase=0", sentinelAddresses.Split(","));
            //初始化 RedisHelper
            RedisHelper.Initialization(csredis);

            while (true)
            {
                try
                {
                    RedisHelper.Set("csRedis_sentinel", "使用CSRedis做哨兵模式的高可用，但不支持读写分离");
                    var value = RedisHelper.Get("csRedis_sentinel");
                    Console.WriteLine(value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                Console.ReadLine();
            }
        }

        /// <summary>
        /// FreeRedis的主从复制+读写分离模式，支持读写分离，但不支持高可用，如果主库宕机切换，FreeRedis不会自动切换主库，
        /// 还有一个弊端是读从库时的连接是随机的，可能内部没有负载均衡（需要研究源码），实测情况是分发不均
        /// </summary>
        static void FreeRedisMasterSlaveTest()
        {
            // 根据官方说明：写入时连接第一个，读取时随机连接第二个到后面的，也就是连接列表的第一个为主库，后面的为从库
            var cli = new FreeRedis.RedisClient(
                "192.168.2.130:6379,password=bb123456,defaultDatabase=1",
                "192.168.2.148:6379,password=bb123456,defaultDatabase=1",
                "192.168.2.149:6379,password=bb123456,defaultDatabase=1"
                );

            while (true)
            {
                try
                {
                    cli.Set("freeRedis_masterslave", "使用freeRedis做主从复制+读写分离");
                    var value = cli.Get("freeRedis_masterslave");
                    Console.WriteLine(value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                Console.ReadLine();
            }
        }

        /// <summary>
        /// FreeRedis的哨兵模式，支持高可用，支持读写分离，读的时候不清楚内部是否做了负载均衡（实测情况是分发不均）
        /// </summary>
        static void FreeRedisSentinelTest()
        {
            // 哨兵的地址
            var sentinelAddresses = "192.168.2.130:26379,192.168.2.148:26379,192.168.2.149:26379";

            var cli = new FreeRedis.RedisClient(
                "mymaster,password=bb123456,defaultDatabase=1",
                sentinelAddresses.Split(","),
                true
                );

            while (true)
            {
                try
                {
                    cli.Set("freeRedis_sentinel", "使用freeRedis的哨兵模式做读写分离+高可用");
                    var value = cli.Get("freeRedis_sentinel");
                    Console.WriteLine(value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                Console.ReadLine();
            }
        }

        /// <summary>
        /// 基于redis一主二从三哨兵+Haproxy反向代理的高可用环境实现的读写分离，
        /// 优点：支持读写分离，支持高可用（环境内部实现），支持负载均衡（实测分发均匀）
        /// 缺点：在主库切换时有一定的断连时间，不太好判断是正在主从切换，还是整个高可用环境挂了（这个问题应该各大三方库都有）
        /// </summary>
        static void HaproxyTest()
        {
            while (true)
            {
                try
                {
                    // 创建写的Redis连接
                    ConnectionMultiplexer writeConn = ConnectionMultiplexer.Connect("192.168.2.130:16379,password=bb123456,defaultDatabase=2");
                    IDatabase writeDB = writeConn.GetDatabase();

                    // 设置键值对
                    writeDB.StringSet("stackExchange_Redis_haproxy",
                        "使用StackExchange.Redis的库连接redis，读写分离通过自己区分连接来做，具体的读写转发由redis高可用环境实现");


                    // 创建读的Redis连接
                    ConnectionMultiplexer readConn = ConnectionMultiplexer.Connect("192.168.2.130:16378,password=bb123456,defaultDatabase=2");
                    IDatabase readDB = readConn.GetDatabase();

                    // 获取键的值
                    string value = readDB.StringGet("stackExchange_Redis_haproxy");
                    Console.WriteLine(value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                Console.ReadLine();
            }
        }

        /// <summary>
        /// StackExchangeRedis的哨兵模式，支持高可用，但不支持读写分离，读写均走主库
        /// </summary>
        static void StackExchangeRedisSentinelTest()
        {
            while (true)
            {
                try
                {
                    // 创建 Redis Sentinel 连接
                    ConfigurationOptions config = new ConfigurationOptions
                    {
                        // 设置 Redis Sentinel 的地址和端口
                        EndPoints = { "192.168.2.130:26379", "192.168.2.148:26379", "192.168.2.149:26379" },
                        // 设置 Redis 主节点名称
                        ServiceName = "mymaster",
                        // 开启读写分离
                        AllowAdmin = false,
                        TieBreaker = "__Booksleeve_TieBreak",
                        // 设置密码
                        Password = "bb123456",
                        DefaultDatabase = 2
                        // 其他配置...
                    };

                    ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(config);

                    IDatabase database = connection.GetDatabase();
                    // 写操作（发送到主节点）
                    database.StringSet("stackExchange_redis_sentinel", "使用StackExchange.Redis库的哨兵模式做高可用");

                    // 读操作（发送到从节点）
                    string value = database.StringGet("stackExchange_redis_sentinel");

                    Console.WriteLine(value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                Console.ReadLine();
            }
        }
    }
}