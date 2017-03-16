using A.RabbitMQ.Client.Model;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace A.RabbitMQ.Client
{
    public delegate TResponse CallBackEvent<TRequest, TResponse>(TRequest msg);

    public class RpcServer : IDisposable
    {
        private RabbitContext _context;

        public string ListenQueueName { get; set; } = "Presbyter.Default";
        public string ExchangeName { get; set; } = "";

        public RpcServer(string uri)
        {
            _context = new RabbitContext(uri);
        }

        public void Respond<TRequest, TResponse>(CallBackEvent<TRequest, TResponse> func)
            where TRequest : RequestBase
            where TResponse : ResponseBase
        {
            _context.Channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, false, true);
            _context.Channel.QueueDeclare(ListenQueueName, false, false, true);
            _context.Channel.QueueBind(ListenQueueName, ExchangeName, $"{ExchangeName}.{ListenQueueName}");

            _context.Channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(_context.Channel);
            _context.Channel.BasicConsume(queue: ListenQueueName,
              noAck: false, consumer: consumer);

            consumer.Received += (model, ea) =>
            {
                ResponseBase response = null;

                var body = ea.Body;
                var props = ea.BasicProperties;
                var replyProps = _context.Channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                try
                {
                    var message = Encoding.UTF8.GetString(body);
                    if (null != func)
                    {
                        response = func.Invoke(JsonConvert.DeserializeObject<TRequest>(message));
                    }
                }
                catch (Exception e)
                {
                    response = null;
                }
                finally
                {
                    var responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                    _context.Channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                      basicProperties: replyProps, body: responseBytes);
                    _context.Channel.BasicAck(deliveryTag: ea.DeliveryTag,
                      multiple: false);
                }
            };
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
