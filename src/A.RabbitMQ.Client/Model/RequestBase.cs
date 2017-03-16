using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A.RabbitMQ.Client.Model
{
    public abstract class RequestBase
    {
        public string ServiceName { get; set; }
        public string FunctionName { get; set; }
        //public object Data { get; set; }
    }
}
