using System;
using System.Text;
using EasyNetQ.AMQP;

namespace EasyNetQ.Patterns
{
    public class EasyNetQBus
    {
        private readonly IProducerPipelineBuilder producerPipelineBuilder;
        private readonly IPersistentConnection persistentConnection;
        private readonly IPersistentChannel persistentChannel;
        private readonly IPublishDispatcher publishDispatcher;

        private readonly IConsumerPipelineBuilder consumerPipelineBuilder;

        public EasyNetQBus(
            IProducerPipelineBuilder producerPipelineBuilder, 
            IPersistentConnection persistentConnection, 
            IPersistentChannel persistentChannel, 
            IPublishDispatcher publishDispatcher, 
            IConsumerPipelineBuilder consumerPipelineBuilder)
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
            if(consumerPipelineBuilder == null)
            {
                throw new ArgumentNullException("consumerPipelineBuilder");
            }

            this.producerPipelineBuilder = producerPipelineBuilder;
            this.persistentConnection = persistentConnection;
            this.persistentChannel = persistentChannel;
            this.publishDispatcher = publishDispatcher;
            this.consumerPipelineBuilder = consumerPipelineBuilder;
        }

        public void Publish<T>(T message)
        {
            var pipeline = producerPipelineBuilder.CreateDefaultPublishPipeline(message);
            var context = new MessagePublishingContext();
            pipeline(context);
            publishDispatcher.Publish(context.RawMessage, context.PublishSettings);
        }

        public IConsumerHandle Subscribe<T>(string queuePostfix, Action<T> handler)
        {
            var loop = new QueueingConsumerLoop();
            var handlerSelector = new BasicHandlerSelector();
            var pipeline = consumerPipelineBuilder.CreatePipeline<T>();

            handlerSelector.SetHandler(messageDeliveryContext =>
            {
                var message = pipeline(messageDeliveryContext);
                handler(message);
            });

            var executionPolicyBuilder = new DefaultExecutionPolicyBuilder();

            var consumer = new Consumer(
                loop,
                handlerSelector,
                executionPolicyBuilder
                );

            var subscriptionChannel = new PersistentChannel();

            var persistentConsumer = new PersistentConsumer(persistentConnection, subscriptionChannel);

            var queueSettings = new QueueSettings();
            var queue = Queue.Create("queue_name", queueSettings);

            var consumerSettings = new ConsumerSettings(queue)
            {
                ConsumerTag = Guid.NewGuid().ToString()
            };

            persistentConsumer.StartConsuming(consumer, consumerSettings);

            return null;
        }
    }
}