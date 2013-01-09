using System;
using RabbitMQ.Client;

namespace EasyNetQ.AMQP
{
    public class EasyNetQBasicConsumer : IBasicConsumer
    {
        public IConsumer Consumer { get; private set; }
        public IChannel Channel { get; set; }

        public EasyNetQBasicConsumer(IConsumer consumer, IChannel channel)
        {
            Consumer = consumer;
            Channel = channel;
        }

        public void HandleBasicConsumeOk(string consumerTag)
        {
            Consumer.ConsumeStarted(consumerTag);
        }

        public void HandleBasicCancelOk(string consumerTag)
        {
            Consumer.ConsumeCancelled(consumerTag);
        }

        public void HandleBasicCancel(string consumerTag)
        {
            Consumer.ConsumeCancelled(consumerTag);
        }

        public void HandleModelShutdown(IModel model, ShutdownEventArgs reason)
        {
            // TODO: do we need to handle this? 
            // The persistent connection should rebuild the consumer.
            //  
        }

        public void HandleBasicDeliver(
            string consumerTag, 
            ulong deliveryTag, 
            bool redelivered, 
            string exchange, 
            string routingKey, 
            IBasicProperties properties, 
            byte[] body)
        {
            var messageProperties = PropertyConverter.ConvertFromBasicProperties(properties);
            var messageDeliveryContext = new MessageDeliveryContext(
                consumerTag,
                deliveryTag,
                redelivered,
                exchange,
                routingKey,
                messageProperties,
                body,
                Channel);

            Consumer.HandleMessage(messageDeliveryContext);
        }

        public IModel Model
        {
            get { throw new NotImplementedException(); }
        }
    }
}