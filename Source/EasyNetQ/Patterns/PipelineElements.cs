using System;
using System.Text;
using EasyNetQ.AMQP;
using Newtonsoft.Json;

namespace EasyNetQ.Patterns
{
    public static class PipelineElements
    {
        public static ConsumerTransformer<string> ConvertToString()
        {
            return context => Encoding.UTF8.GetString(context.Body);
        }

        public static ProducerTransformer<byte[]> ConvertToByte(string messageAsJson)
        {
            return context =>
            {
                context.ContentEncoding = "UTF8";
                return Encoding.UTF8.GetBytes(messageAsJson);
            };
        } 

        public static ConsumerTransformer<T> DeSerialize<T>(string body)
        {
            return context => JsonConvert.DeserializeObject<T>(body);
        }

        public static ProducerTransformer<string> Serialize<T>(T message)
        {
            return context =>
            {
                context.MessageType = typeof (T);
                context.ContentType = "application/json";
                return JsonConvert.SerializeObject(message);
            };
        }

        public static ProducerTransformer<IRawMessage> CreateMessage(byte[] bytes)
        {
            return context => new RawMessage(bytes);
        }

        public static ProducerTransformer<IRawMessage> SetMessage(IRawMessage message)
        {
            return context =>
            {
                message.Properties.Type.Value = context.MessageType.Name;
                message.Properties.ContentType.Value = context.ContentType;
                message.Properties.ContentEncoding.Value = context.ContentEncoding;
                context.RawMessage = message;
                return message;
            };
        } 

        public static ProducerTransformer<IRawMessage> SetCorrelationId()
        {
            return context =>
            {
                if (!context.RawMessage.Properties.CorrelationId.IsSet)
                {
                    context.RawMessage.Properties.CorrelationId.Value = Guid.NewGuid().ToString();
                }
                return context.RawMessage;
            };
        }

        public static ProducerTransformer<IRawMessage> SetExchangeAndRoutingKey()
        {
            return context =>
            {
                var exchangeName = TypeNameSerializer.Serialize(context.MessageType);
                var exchange = Exchange.Topic(exchangeName);

                const string routingKey = "#";

                context.PublishSettings = new PublishSettings(exchange, routingKey);
                return context.RawMessage;
            };
        } 
    }
}