using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client.MessagePatterns;
using RabbitMQ.Client;
using Model;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory()
            {
                Uri = "amqp://rabbitmq_admin:123456@192.168.1.25:5672/test"
            };
            // 设置心跳超时，默认值为60秒
            factory.RequestedHeartbeat = 60;
            using (IConnection conn = factory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    // 声明Exchange
                    channel.ExchangeDeclare(
                        exchange: "ExchangeName",
                        type: ExchangeType.Direct,
                        durable: false,
                        autoDelete: false
                        );
                    // 声明Queue
                    var queueName = channel.QueueDeclare(
                        queue: "QueueName",
                        durable: false,
                        exclusive: false,
                        autoDelete: false
                        ).QueueName;
                    // 绑定Queue
                    channel.QueueBind(
                        queue: queueName,
                        exchange: "ExchangeName",
                        routingKey: "RoutingKey" // direct连接，RoutingKey可以省略
                        );

                    Subscription subscription = new Subscription(channel, queueName);
                    RpcServer server = new RpcServer(subscription);

                    server.MainLoop();

                    Console.ReadLine();
                }
            }
        }
    }

    public class RpcServer : SimpleRpcServer
    {
        public RpcServer(Subscription subscription) : base(subscription)
        {
        }

        public override byte[] HandleSimpleCall(bool isRedelivered, IBasicProperties requestProperties, byte[] body, out IBasicProperties replyProperties)
        {
            replyProperties = requestProperties;
            replyProperties.MessageId = Guid.NewGuid().ToString().ToUpper();

            string bodyStr = Encoding.GetEncoding(requestProperties.ContentEncoding).GetString(body);
            Console.WriteLine(bodyStr);
            RabbitReq request = DeserializeRequest(requestProperties.ContentType, bodyStr);
            if (null != request)
            {
                RabbitRes response = null;
                switch (request.FunctionName)
                {
                    case "Login":
                        // do something
                        var data = new Dictionary<string, object>();
                        data.Add("result", $"{request.Parameters["Account"]} {request.Parameters["Password"]} done.");
                        response = new RabbitRes()
                        {
                            Code = 200,
                            Description = "Success",
                            Data = data
                        };
                        break;
                    default:
                        break;
                }
                if (null != response)
                {
                    return SerializeResponse(requestProperties.ContentType, requestProperties.ContentEncoding, response);
                }
            }
            return null;
        }

        private RabbitReq DeserializeRequest(string contentType, string body)
        {
            RabbitReq request = null;
            switch (contentType.ToLower())
            {
                case "application/json":
                    request = JsonConvert.DeserializeObject<RabbitReq>(body);
                    break;
                case "text/xml":
                    // todo
                    break;
                default:
                    request = JsonConvert.DeserializeObject<RabbitReq>(body);
                    break;
            }
            return request;
        }

        private byte[] SerializeResponse(string contentType, string encodingType, RabbitRes response)
        {
            string str = null;
            switch (contentType.ToLower())
            {
                case "application/json":
                    str = JsonConvert.SerializeObject(response);
                    break;
                case "text/xml":
                    // todo
                    break;
                default:
                    str = JsonConvert.SerializeObject(response);
                    break;
            }
            if (null != str)
            {
                return Encoding.GetEncoding(encodingType).GetBytes(str);
            }
            return null;
        }
    }
}
