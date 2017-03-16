using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A.RabbitMQ.Client.Model
{
    public class RabbitRes<T> : ResponseBase
    {
        public T Data { get; set; }
    }
}
