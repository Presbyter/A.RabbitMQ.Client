using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A.RabbitMQ.Client.Model
{
    public abstract class ResponseBase
    {
        public int StatusCode { get; set; }
        public string Desc { get; set; }
        //public object Data { get; set; }
    }
}
