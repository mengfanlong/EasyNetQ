using System;

namespace EasyNetQ.AMQP
{
    /// <summary>
    /// Represents a thread safe publisher. It is initialized with a single channel, but
    /// Publish can be called from different threads.
    /// </summary>
    public interface IPublishDispatcher : IDisposable
    {
        /// <summary>
        /// Set the channel to publish on.
        /// </summary>
        void Initialize(IPersistentConnection connection, IChannelSettings channelSettings);

        /// <summary>
        /// Thread-safe publish method
        /// </summary>
        /// <param name="message"></param>
        /// <param name="settings"></param>
        void Publish(IRawMessage message, IPublishSettings settings);
    }
}