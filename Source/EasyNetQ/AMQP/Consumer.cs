using System;

namespace EasyNetQ.AMQP
{
    public interface IConsumer
    {
        IConsumerLoop ConsumerLoop { get; }
        void ConsumeStarted(string consumerTag);
        void ConsumeCancelled(string consumerTag);
        void HandleMessage(IMessageDeliveryContext messageDeliveryContext);
    }

    /// <summary>
    /// Consumer handles message deliery calls from RabbitMQ.
    /// Dispatches the correct handler, and passes the handler to the 
    /// consumer loop.
    /// </summary>
    public class Consumer : IConsumer
    {
        public IConsumerLoop ConsumerLoop { get; private set; }
        public IHandlerSelector HandlerSelector { get; private set; }
        public IExecutionPolicyBuilder ExecutionPolicyBuilder { get; private set; }

        private readonly Action<IMessageDeliveryContext> executionPolicy;

        public Consumer(
            IConsumerLoop consumerLoop, 
            IHandlerSelector handlerSelector, 
            IExecutionPolicyBuilder executionPolicyBuilder)
        {
            if(consumerLoop == null)
            {
                throw new ArgumentNullException("consumerLoop");
            }
            if(handlerSelector == null)
            {
                throw new ArgumentNullException("handlerSelector");
            }
            if(executionPolicyBuilder == null)
            {
                throw new ArgumentNullException("executionPolicyBuilder");
            }

            ConsumerLoop = consumerLoop;
            HandlerSelector = handlerSelector;
            ExecutionPolicyBuilder = executionPolicyBuilder;

            executionPolicy = ExecutionPolicyBuilder.BuildExecutionPolicy(HandlerSelector.GetHandler);
        }

        public void ConsumeStarted(string consumerTag)
        {
            ConsumerLoop.Start(consumerTag);
        }

        public void ConsumeCancelled(string consumerTag)
        {
            ConsumerLoop.Stop(consumerTag);
        }

        public void HandleMessage(IMessageDeliveryContext messageDeliveryContext)
        {
            if(messageDeliveryContext == null)
            {
                throw new ArgumentNullException("messageDeliveryContext");
            }

            ConsumerLoop.QueueMessageHandleAction(() => executionPolicy(messageDeliveryContext));
        }
    }
}