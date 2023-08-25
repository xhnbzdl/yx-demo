using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.WebSocket4Net;
using MQTTnet.Formatter;

namespace EMQX.Publish
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Publish_Application_Message();
            //await Connect_Client_Using_MQTTv5();
            //await Connect_Client_Using_WebSockets();
            //await Connect_Client_Using_WebSocket4Net();
        }

        public static async Task Publish_Application_Message()
        {
            /*
             * 本示例推送一个简单的应用程序消息，包括一个主题和一个有效载荷。
             * 在有构建器的地方一定要使用构建器。构建器（在本项目中）旨在向后兼容。             
             * 也支持通过_MqttApplicationMessage_的构造函数创建_MqttApplicationMessage_。
             * 但在未来的版本中，该类可能会经常变化，而构建器不会
             * 或至少在可能的情况下提供向后兼容性。
             */

            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer("192.168.2.131")
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("samples/temperature/living_room")
                    .WithPayload("19.5")
                    .Build();

                for (int i = 0; i < 10; i++)
                {
                    await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                    Console.ReadLine();
                }


                Console.WriteLine("MQTT application message is published.");

                Console.ReadKey();

                await mqttClient.DisconnectAsync();
            }
        }

        #region 连接示例 https://github.com/dotnet/MQTTnet/blob/master/Samples/Client/Client_Connection_Samples.cs
        public static async Task Connect_Client_Using_WebSocket4Net()
        {
            /*
             * 本示例创建了一个简单的 MQTT 客户端，并使用 WebSocket 连接连接到公共代理。
             * 本示例使用 WebSocket4Net 的实现，而不是 .NET 的 WebSockets 实现。它提供了更多
             * 加密算法并支持更多平台。
             * 
             * 这是 _Connect_Client_ 示例的修改版本！更多详情，请参阅其他示例。
             */

            var mqttFactory = new MqttFactory().UseWebSocket4Net();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithWebSocketServer("192.168.2.131:8083/mqtt")
                    .Build();

                var response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                Console.WriteLine("The MQTT client is connected.");

                response.DumpToConsole();

                Console.ReadKey();
            }
        }

        public static async Task Connect_Client_Using_WebSockets()
        {
            /*
             * 本示例创建了一个简单的 MQTT 客户端，并使用 WebSocket 连接连接到公共代理。
             * 
             * 这是 _Connect_Client_ 示例的修改版本！更多详情，请参阅其他示例。
             */

            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithWebSocketServer("192.168.2.131:8083/mqtt")
                    .Build();

                var response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                Console.WriteLine("The MQTT client is connected.");

                response.DumpToConsole();

                Console.ReadKey();
            }
        }

        public static async Task Connect_Client_Using_MQTTv5()
        {
            /*
             * 该示例创建了一个简单的 MQTT 客户端，并使用 MQTTv5 连接到公共代理。
             */

            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer("192.168.2.131", 1883)
                    .WithProtocolVersion(MqttProtocolVersion.V500)
                    .Build();

                // In MQTTv5 the response contains much more information.
                var response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                Console.WriteLine("The MQTT client is connected.");

                response.DumpToConsole();

                Console.ReadKey();
            }
        }
        #endregion
    }
}