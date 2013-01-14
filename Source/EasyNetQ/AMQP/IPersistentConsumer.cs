using System;

namespace EasyNetQ.AMQP
{
    public interface IPersistentConsumer : IDisposable
    {
        void StartConsuming(IConsumer consumer, ConsumerSettings settings, ChannelSettings channelSettings);
    }
}