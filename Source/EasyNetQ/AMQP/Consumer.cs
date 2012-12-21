using System;

namespace EasyNetQ.AMQP
{
    public interface IConsumer
    {
        IConsumerLoop ConsumerLoop { get; }
        ConsumerSettings Settings { get; }
    }

    public class Consumer : IConsumer
    {
        public IConsumerLoop ConsumerLoop { get; private set; }
        public ConsumerSettings Settings { get; private set; }

        public Consumer(IConsumerLoop consumerLoop, ConsumerSettings settings)
        {
            if(consumerLoop == null)
            {
                throw new ArgumentNullException("consumerLoop");
            }
            if(settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            ConsumerLoop = consumerLoop;
            Settings = settings;
        }
    }
}