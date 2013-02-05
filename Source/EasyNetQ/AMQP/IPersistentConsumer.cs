using System;

namespace EasyNetQ.AMQP
{
    public interface IPersistentConsumer : IDisposable
    {
        IConsumerHandle StartConsuming(IConsumer consumer, ConsumerSettings settings, ChannelSettings channelSettings);
        IConsumerHandle StartConsuming(IConsumer consumer, ConsumerSettings settings);
    }
}