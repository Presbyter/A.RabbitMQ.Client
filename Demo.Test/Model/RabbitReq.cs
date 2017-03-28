using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class RabbitReq
    {
        public RabbitReq()
        {
            if (null == Parameters)
            {
                Parameters = new Dictionary<string, object>();
            }
        }

        public string FunctionName { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}
