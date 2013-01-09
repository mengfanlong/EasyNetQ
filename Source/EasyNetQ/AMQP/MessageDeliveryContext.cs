using System;

namespace EasyNetQ.AMQP
{
    public interface IMessageDeliveryContext
    {
        string ConsumerTag { get; }
        ulong DeliveryTag { get; }
        bool Redelivered { get; }
        string Exchange { get; }
        string RoutingKey { get; }
        IMessageProperties Properties { get; }
        byte[] Body { get; }
        IChannel Channel { get; }
    }

    public class MessageDeliveryContext : IMessageDeliveryContext
    {
        public string ConsumerTag { get; private set; }
        public ulong DeliveryTag { get; private set; }
        public bool Redelivered { get; private set; }
        public string Exchange { get; private set; }
        public string RoutingKey { get; private set; }
        public IMessageProperties Properties { get; private set; }
        public byte[] Body { get; private set; }
        public IChannel Channel { get; private set; }

        public MessageDeliveryContext(
            string consumerTag, 
            ulong deliveryTag, 
            bool redelivered, 
            string exchange, 
            string routingKey, 
            IMessageProperties properties, 
            byte[] body, 
            IChannel channel)
        {
            if(consumerTag == null)
            {
                throw new ArgumentNullException("consumerTag");
            }
            if(exchange == null)
            {
                throw new ArgumentNullException("exchange");
            }
            if(routingKey == null)
            {
                throw new ArgumentNullException("routingKey");
            }
            if(properties == null)
            {
                throw new ArgumentNullException("properties");
            }
            if(body == null)
            {
                throw new ArgumentNullException("body");
            }
            if(channel == null)
            {
                throw new ArgumentNullException("channel");
            }

            ConsumerTag = consumerTag;
            DeliveryTag = deliveryTag;
            Redelivered = redelivered;
            Exchange = exchange;
            RoutingKey = routingKey;
            Properties = properties;
            Body = body;
            Channel = channel;
        }
    }
}