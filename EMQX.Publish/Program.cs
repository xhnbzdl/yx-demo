using MQTTnet;
using MQTTnet.Client;

namespace EMQX.Publish
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Publish_Application_Message();
        }

        public static async Task Publish_Application_Message()
        {
            /*
             * This sample pushes a simple application message including a topic and a payload.
             *
             * Always use builders where they exist. Builders (in this project) are designed to be
             * backward compatible. Creating an _MqttApplicationMessage_ via its constructor is also
             * supported but the class might change often in future releases where the builder does not
             * or at least provides backward compatibility where possible.
             */

            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer("192.168.2.131",1883)
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
    }
}