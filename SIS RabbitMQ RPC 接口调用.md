# RabbitMQ RPC 接口调用 统一样例

## 创建MQ服务器的Connection连接(确保只连接一次)
## 创建信道
## 使用服务提供方给出的Exchange名称,与相应的RoutingKey名称,创建client
``` csharp
var client = new SimpleRpcClient(channel, "IHousingResource", ExchangeType.Direct, "B1");
```
1. 第一个参数是以上所创建的信道
1. 第二个参数是Exchange名称
1. 第三个参数请使用Direct连接(RPC模型选用直连方式)
1. 第四个参数为RoutingKey. RoutingKey规定为AreaCode字符串值. 例如 "B400"

## 打包所要发送的数据
``` csharp
// 测试数据 获取字典项
var request = new RabbitReq();
request.FunctionName = "GetFieldConfigNode";
var paras = new Dictionary<string, object> {{"AreaCode", "B400"}};
request.Parameters = JsonConvert.SerializeObject(paras);

var msgBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
var prop = new BasicProperties();
prop.ContentType = "application/json";
prop.ContentEncoding = "UTF-8";
```
1. RabbitReq类中,封装了String类型的FunctionName属性,与String类型的Parameters属性
1. FunctionName为接口文档所提供的方法名称
1. Parameters的结构,是接口文档所提供的请求实体,请参照接口文档
1. 目前现阶段规定只能序列化为Json格式
1. 为了统一字符集编码,请尽量使用UTF-8字符集

## 发送到MQ,并获取返回Byte[]
``` csharp
IBasicProperties replyProp = new BasicProperties();
var responseBytes = client.Call(prop, msgBytes, out replyProp);
```
1. 返回的结果为Byte数组
1. 字符编码方式为上面发送消息时所定的 prop.ContentEncoding = "UTF-8";
1. 序列化方式为上面发送消息时所规定的 prop.ContentType = "application/json";
1. 其结构请参照接口文档

## 以下为完整参考代码
``` csharp
private static void Main(string[] args)
{
    var input = "";
    var factory = new ConnectionFactory
    {
        Uri = "amqp://rabbitmq_admin:123456@192.168.1.25:5672/test"
    };
    // 心跳超时，默认60秒
    factory.RequestedHeartbeat = 60;
    using (var conn = factory.CreateConnection())
    {
        using (var channel = conn.CreateModel())
        {
            while ("exit" != (input = Console.ReadLine()))
            {
                var client = new SimpleRpcClient(channel, "IHousingResource", ExchangeType.Direct, "B1");

                // 测试数据 获取字典项
                var request = new RabbitReq();
                request.FunctionName = "GetFieldConfigNode";
                var paras = new Dictionary<string, object> {{"AreaCode", "B400"}};
                request.Parameters = JsonConvert.SerializeObject(paras);

                var msgBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
                var prop = new BasicProperties();
                prop.ContentType = "application/json";
                prop.ContentEncoding = "UTF-8";

                IBasicProperties replyProp = new BasicProperties();
                var responseBytes = client.Call(prop, msgBytes, out replyProp);

                var responseStr = Encoding.UTF8.GetString(responseBytes);
                var response = JsonConvert.DeserializeObject<RabbitRes>(responseStr);
                Console.WriteLine(responseStr);
            }
        }
    }
}
```