using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace RabbitConsumerCore
{
    class Program
    {
        private static void Enqueue(string json)
        {
            // push message into rabbit
            try
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                //factory.Port = 5677;
                //factory.HostName = "rabbit1";
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "mq_read_inbox",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var body = Encoding.UTF8.GetBytes(json);
                    channel.BasicPublish(exchange: "",
                                         routingKey: "mq_read_inbox",
                                         basicProperties: null,
                                         body: body);

                    Console.WriteLine(" [x] Sent {0}", json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"not ready ... {ex}");
            }
        }

        static void HandleRequest(object model, BasicDeliverEventArgs e)
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            //Console.WriteLine(" [x] Received {0}", message);

            Task.Run(() =>
            {
                Console.WriteLine(" Worker executing " + message);
                Message msg = JsonConvert.DeserializeObject<Message>(message);
                Console.WriteLine("fire stored procedure...");
                
                msg.db_rows = "db rows...";

                //string json = $"{{ corr_id: '{msg.corr_id}', sp_name: '{msg.sp_name}', result: 'db rows' }}";

                string json = JsonConvert.SerializeObject(msg);
                Enqueue(json);
            });
        }
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            //factory.Port = 5677;
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "mq_read_outbox",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var consumer = new EventingBasicConsumer(channel);

                    consumer.Received += HandleRequest;

                    channel.BasicConsume(queue: "mq_read_outbox",
                                         autoAck: true,
                                         consumer: consumer);
                    // Before Dispose!!!
                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }

        }
    }
}
