using Model;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.Demo.Publish
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
                    string input = string.Empty;
                    while ("exit" != (input = Console.ReadLine()))
                    {
                        RabbitReq request = new RabbitReq();
                        request.Data.Add("column1", input);
                        request.Data.Add("column2", "test str 2");
                        request.Data.Add("column3", new { Account = "xingchao", Password = "123456" });
                        var bodyBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));

                        IBasicProperties prop = new BasicProperties();
                        prop.ContentType = "application/json";
                        prop.ContentEncoding = "UTF-8";

                        channel.BasicPublish("publish", "", prop, bodyBytes);
                    }
                }
            }
        }
    }
}
