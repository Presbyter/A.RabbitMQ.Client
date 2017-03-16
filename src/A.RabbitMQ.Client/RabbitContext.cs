using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A.RabbitMQ.Client
{
    public class RabbitContext
    {
        public IConnection Connection { get; set; }
        public IModel Channel { get; set; }

        public RabbitContext(string uri)
        {
            Connection = RabbitFactory.CreateConnection(uri);
            Channel = RabbitFactory.CreateChannel(Connection);
        }
    }
}
