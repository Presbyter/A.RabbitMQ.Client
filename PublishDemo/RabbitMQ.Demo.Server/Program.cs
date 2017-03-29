using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.Demo.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory()
            {
                Uri = "amqp://rabbitmq_admin:123456@192.168.1.25:5672/test"
            };
            using (IConnection conn = factory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    // 声明Exchange
                    channel.ExchangeDeclare(
                        exchange: "publish",
                        type: ExchangeType.Fanout,
                        durable: false,
                        autoDelete: false
                        );
                    // 声明Queue
                    var queueName = channel.QueueDeclare(
                        queue: "publish.demo1",
                        durable: false,
                        exclusive: false,
                        autoDelete: false
                        ).QueueName;
                    // 绑定Queue
                    channel.QueueBind(
                        queue: queueName,
                        exchange: "publish",
                        routingKey: ""
                        );

                    Subscription sub = new Subscription(channel, "publish.demo1");
                    foreach (BasicDeliverEventArgs ea in sub)
                    {
                        var bodyBytes = ea.Body;
                        string bodyStr = Encoding.GetEncoding(ea.BasicProperties.ContentEncoding).GetString(bodyBytes);
                        Console.WriteLine(bodyStr);
                        // do something
                        Console.WriteLine($"{queueName} done!");
                    }
                }
            }
        }
    }
}
