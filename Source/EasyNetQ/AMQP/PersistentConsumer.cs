using System;
using System.Collections.Generic;

namespace EasyNetQ.AMQP
{
    public class PersistentConsumer : IPersistentConsumer
    {
        private readonly IPersistentConnection connection;
        private readonly IList<IChannel> openChannels = new List<IChannel>();

        public PersistentConsumer(IPersistentConnection connection)
        {
            this.connection = connection;
        }

        public IConsumerHandle StartConsuming(IConsumer consumer, ConsumerSettings settings)
        {
            return StartConsuming(consumer, settings, new ChannelSettings());
        }

        public IConsumerHandle StartConsuming(IConsumer consumer, ConsumerSettings settings, ChannelSettings channelSettings)
        {
            var consumerHandle = new PersistentConsumerHandle();
            StartConsumingInternal(consumer, settings, channelSettings, consumerHandle);
            return consumerHandle;
        }

        private void StartConsumingInternal(
            IConsumer consumer, 
            ConsumerSettings settings, 
            ChannelSettings channelSettings,
            PersistentConsumerHandle consumerHandle)
        {
            if (disposed)
            {
                throw new EasyNetQAmqpException("PersistentConsumer is disposed");
            }

            try
            {
                var channel = connection.OpenChannel(channelSettings);
                Action channelClosedHandler = null;
                channelClosedHandler = () =>
                {
                    channel.ChannelClosed -= channelClosedHandler;
                    channel.Dispose();
                    openChannels.Remove(channel);
                    StartConsumingInternal(consumer, settings, channelSettings, consumerHandle);
                };
                channel.ChannelClosed += channelClosedHandler;

                consumerHandle.SetHandle(channel.StartConsuming(consumer, settings));
                openChannels.Add(channel);

            }
            catch (Exception)
            {
                Action connectionOpenHandler = null;
                connectionOpenHandler = () =>
                {
                    connection.Connected -= connectionOpenHandler;
                    StartConsumingInternal(consumer, settings, channelSettings, consumerHandle);
                };
                connection.Connected += connectionOpenHandler;
            }
        }

        private bool disposed = false;

        public void Dispose()
        {
            disposed = true;
            foreach (var openChannel in openChannels)
            {
                openChannel.Dispose();
            }
        }
    }

    public class PersistentConsumerHandle : IConsumerHandle
    {
        private IConsumerHandle consumerHandle = null;

        public void Dispose()
        {
            if (consumerHandle != null)
            {
                consumerHandle.Dispose();
            }
        }

        public void SetHandle(IConsumerHandle consumerHandle)
        {
            this.consumerHandle = consumerHandle;
        }
    }
}