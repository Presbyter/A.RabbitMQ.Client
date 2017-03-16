using A.RabbitMQ.Client.Model;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace A.RabbitMQ.Client
{
    public class RpcClient : IDisposable
    {
        private RabbitContext _context;
        private QueueingBasicConsumer _consumer;
        private string _replayQueueName;

        public string SendQueueName { get; set; } = "Presbyter.Default";
        public string ExchangeName { get; set; } = "";

        public RpcClient(string uri)
        {
            _context = new RabbitContext(uri);
        }

        private void SendInit()
        {
            _replayQueueName = _context.Channel.QueueDeclare().QueueName;
            _consumer = new QueueingBasicConsumer(_context.Channel);
            _context.Channel.BasicConsume(_replayQueueName, true, _consumer);
        }

        public TResponse Request<TRequest, TResponse>(TRequest requestMsg)
            where TRequest : RequestBase
            where TResponse : ResponseBase
        {
            SendInit();

            var corrId = Guid.NewGuid().ToString();
            var props = _context.Channel.CreateBasicProperties();
            props.ReplyTo = _replayQueueName;
            props.CorrelationId = corrId;

            var messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestMsg));
            _context.Channel.BasicPublish(exchange: ExchangeName,
                                 routingKey: $"{ExchangeName}.{SendQueueName}",
                                 basicProperties: props,
                                 body: messageBytes);

            while (true)
            {
                var ea = (BasicDeliverEventArgs)_consumer.Queue.Dequeue();
                if (ea.BasicProperties.CorrelationId == corrId)
                {
                    var responseStr = Encoding.UTF8.GetString(ea.Body);
                    return JsonConvert.DeserializeObject<TResponse>(responseStr);
                }
            }
        }

        public void Dispose()
        {
            if (null != _context)
            {
                _context.Channel.Close();
                _context.Connection.Close();
            }
        }
    }
}
