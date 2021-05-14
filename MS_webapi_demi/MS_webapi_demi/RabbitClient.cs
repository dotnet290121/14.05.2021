using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MS_webapi_demi
{
    public static class RabbitClient
    {

        #region listener

        static void HandleRequest(object model, BasicDeliverEventArgs e)
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            //Console.WriteLine(" [x] Received {0}", message);

            // runs on its own thread
            //string corr_id = message; // fix this

            Message msg = JsonConvert.DeserializeObject<Message>(message);

            if (m_requests.ContainsKey(msg.corr_id))
            {
                // free correlated thread
                lock (m_requests[msg.corr_id].Key)
                {
                    m_requests[msg.corr_id].Result = message;
                    Monitor.Pulse(m_requests[msg.corr_id].Key);
                }
            }
            else
            {
                Console.WriteLine($"Message from previous run {msg.corr_id}");
            }

        }
        static void StartListening()
        {

        }

        #endregion

        public class Request
        {
            public object Key { get; set; }
            public string Result { get; set; }
        }
        private static ConcurrentDictionary<string, Request> m_requests = new ConcurrentDictionary<string, Request>();

        private static IConnection m_connection;
        private static IModel m_channel;

        static RabbitClient()
        {
            // connect to rabbit
            // attach listener
            var factory = new ConnectionFactory() { HostName = "localhost" };
            //factory.Port = 5677;

            m_connection = factory.CreateConnection();
            m_channel = m_connection.CreateModel();

            m_channel.QueueDeclare(queue: "mq_read_inbox",
                         durable: false,
                         exclusive: false,
                         autoDelete: false,
                         arguments: null);

            var consumer = new EventingBasicConsumer(m_channel);

            consumer.Received += HandleRequest;

            m_channel.BasicConsume(queue: "mq_read_inbox",
                                 autoAck: true,
                                 consumer: consumer);
        }

        private static void Enqueue(string json)
        {
            // push message into rabbit
            //factory.Port = 5677;
            //factory.HostName = "rabbit1";
            m_channel.QueueDeclare(queue: "mq_read_outbox",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

            var body = Encoding.UTF8.GetBytes(json);
            m_channel.BasicPublish(exchange: "",
                                         routingKey: "mq_read_outbox",
                                         basicProperties: null,
                                         body: body);

            Console.WriteLine(" [x] Sent {0}", json);
        }

        public static string StartRead(string spname, params string[] args)
        {
            // convert sp_name + args to json
            string json = "";
            string corrid = Guid.NewGuid().ToString();
            Message mesage = new Message
            {
                corr_id = corrid,
                sp_name = spname,
                db_rows = ""
            };
            json = JsonConvert.SerializeObject(mesage);
            Enqueue(json);

            object current_key = new object();
            // start wait
            while (!m_requests.TryAdd(mesage.corr_id, new Request { Key = current_key, Result = null }))
            {
                Thread.Sleep(50);
            }

            lock (current_key)
            {
                // check maybe response already here before wait...
                Monitor.Wait(current_key);

                // reponse is here
                string result = m_requests[mesage.corr_id].Result; // parse the json into list<dic<string, obj>>
                //m_requests.TryRemove(mesage.corr_id, out Request req);


                Console.WriteLine($"Reponse {spname} from rabbit completed....");

                return result;

            }
        }
    }
}
