using System;

namespace EasyNetQ.AMQP
{
    public interface IPersistentConsumer : IDisposable
    {
        IConsumerHandle StartConsuming(IConsumer consumer, IConsumerSettings settings, IChannelSettings channelSettings);
        IConsumerHandle StartConsuming(IConsumer consumer, IConsumerSettings settings);
    }
}