using Model;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using RabbitMQ.Client.MessagePatterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Demo.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = "";
            var factory = new ConnectionFactory()
            {
                Uri = "amqp://rabbitmq_admin:123456@192.168.1.25:5672/test"
            };
            // 心跳超时，默认60秒
            factory.RequestedHeartbeat = 60;
            using (IConnection conn = factory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    while ("exit" != (input = Console.ReadLine()))
                    {
                        var msgBody = new RabbitReq();
                        msgBody.FunctionName = "Login";
                        msgBody.Parameters.Add("Account", "xingchao");
                        msgBody.Parameters.Add("Password", "123456");
                        var msgBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msgBody));
                        var prop = new BasicProperties();
                        prop.ContentType = "application/json";
                        prop.ContentEncoding = "UTF-8";

                        //var client = new SimpleRpcClient(channel, "ExchangeName", ExchangeType.Direct, "RoutingKey");
                        var client = new SimpleRpcClient(channel, "QueueName");
                        IBasicProperties replyProp = new BasicProperties();
                        var replyMsgBytes = client.Call(prop, msgBytes, out replyProp);

                        var response = JsonConvert.DeserializeObject<RabbitRes>(Encoding.UTF8.GetString(replyMsgBytes));

                        Console.WriteLine(JsonConvert.SerializeObject(response));
                    }
                }
            }
        }
    }
}
