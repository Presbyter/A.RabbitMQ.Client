using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A.RabbitMQ.Client
{
    public class RabbitFactory
    {
        public static IConnection CreateConnection(string uri)
        {
            var factory = new ConnectionFactory() { Uri = uri };
            var connection = factory.CreateConnection();
            return connection;
        }

        public static IModel CreateChannel(IConnection conn)
        {
            var channel = conn.CreateModel();
            return channel;
        }
    }
}
