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
            if (null == Data)
            {
                Data = new Dictionary<string, object>();
            }
        }
        public Dictionary<string, object> Data { get; set; }
    }
}
