using NewLife.Log;
using NewLife.RocketMQ;
using NewLife.RocketMQ.Protocol;

namespace RocketMQ.Consumers
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ConsumerData();
            Console.Read();
        }
        /// <summary>
        /// 消费者
        /// </summary>
        private static void ConsumerData()
        {
            var consumer = new Consumer
            {
                Topic = "Test",
                Group = "测试",
                NameServerAddress = "192.168.2.240:9876",
                BatchSize = 20,
                Log = XTrace.Log,
            };

            consumer.OnConsume = (q, ms) =>
            {
                //string mInfo = $"BrokerName={q.BrokerName},QueueId={q.QueueId},Length={ms.Length}";
                //Console.WriteLine(mInfo);
                foreach (var item in ms.ToList())
                {
                    string msg = $"消息：msgId={item.MsgId},key={item.Keys}，产生时间【{item.BornTimestamp.ToDateTime()}】，内容>{System.Text.Encoding.Default.GetString(item.Body)}";
                    Console.WriteLine(msg);
                }
                //   return false;//通知消息队：不消费消息
                return true;        //通知消息队：消费了消息
            };

            consumer.Start();
        }
    }
}