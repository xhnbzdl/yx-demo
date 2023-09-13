using NewLife.Log;
using NewLife.RocketMQ;
using NewLife.RocketMQ.Protocol;
using Newtonsoft.Json;

namespace RocketMQ.Producers
{
    internal class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i <= 20; i++)
            {
                Console.WriteLine("请发布消息");
                string wxMessage = Console.ReadLine();
                ProducerData(wxMessage, i.ToString());
            }
            Console.Read();
        }

        /// <summary>
        /// 生产者
        /// </summary>
        private static void ProducerData(string wxMessage, string key)
        {
            try
            {
                var producer = new Producer
                {
                    Topic = "Test",
                    NameServerAddress = "192.168.2.240:9876",
                    Group = "测试",
                    Log = XTrace.Log,
                };
                producer.Start();


                for (var i = 0; i < 10; i++)
                {
                    var str = "学无先后达者为师" + i;
                    //var str = Rand.NextString(1337);

                    var sr = producer.Publish(str, "TagA");
                }

                //发送消息方式一，可以设置key
                //var msg = new Message()
                //{
                //    BodyString = wxMessage,
                //    Keys = key,
                //    Tags = "TagC",
                //    Flag = 0,
                //    WaitStoreMsgOK = true
                //};
                //var data = producer.Publish(msg);
                ////var data = producer.PublishAsync(msg);
                //Console.WriteLine(JsonConvert.SerializeObject(data));
                ////producer.Publish(JsonConvert.SerializeObject(wxMessage), "测试", "111", 6000);
                //Console.WriteLine("生产者" + JsonConvert.SerializeObject(msg));
                //释放连接
                producer.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("写入消息队列出错：" + ex.ToString());
            }
        }
    }
}