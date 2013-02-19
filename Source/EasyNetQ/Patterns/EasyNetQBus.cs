using System;
using EasyNetQ.AMQP;

namespace EasyNetQ.Patterns
{
    public class EasyNetQBus
    {
        private readonly IProducerPipelineBuilder producerPipelineBuilder;
        private readonly IPersistentConnection persistentConnection;
        private readonly IPersistentChannel persistentChannel;
        private readonly IPublishDispatcher publishDispatcher;

        public EasyNetQBus(
            IProducerPipelineBuilder producerPipelineBuilder, 
            IPersistentConnection persistentConnection, 
            IPersistentChannel persistentChannel, 
            IPublishDispatcher publishDispatcher)
        {
            if(producerPipelineBuilder == null)
            {
                throw new ArgumentNullException("producerPipelineBuilder");
            }
            if(persistentConnection == null)
            {
                throw new ArgumentNullException("persistentConnection");
            }
            if(persistentChannel == null)
            {
                throw new ArgumentNullException("persistentChannel");
            }
            if(publishDispatcher == null)
            {
                throw new ArgumentNullException("publishDispatcher");
            }

            this.producerPipelineBuilder = producerPipelineBuilder;
            this.persistentConnection = persistentConnection;
            this.persistentChannel = persistentChannel;
            this.publishDispatcher = publishDispatcher;
        }

        public void Publish<T>(T message)
        {
            var pipeline = producerPipelineBuilder.CreateDefaultPublishPipeline(message);
            var context = new MessagePublishingContext();
            pipeline(context);
            publishDispatcher.Publish(context.RawMessage, context.PublishSettings);
        }

        public IConsumerHandle Subscribe<T>(string queuePrefix, Action<T> handler)
        {
            return null;
        }
    }
}