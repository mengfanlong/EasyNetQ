using System;

namespace EasyNetQ.AMQP
{
    public class PersistentConsumer : IPersistentConsumer
    {
        private readonly IPersistentConnection persistentConnection;
        private readonly IPersistentChannel persistentChannel;

        public PersistentConsumer(IPersistentConnection persistentConnection, IPersistentChannel persistentChannel)
        {
            if(persistentConnection == null)
            {
                throw new ArgumentNullException("persistentConnection");
            }
            if(persistentChannel == null)
            {
                throw new ArgumentNullException("persistentChannel");
            }

            this.persistentConnection = persistentConnection;
            this.persistentChannel = persistentChannel;
        }

        public IConsumerHandle StartConsuming(IConsumer consumer, IConsumerSettings settings)
        {
            return StartConsuming(consumer, settings, new ChannelSettings());
        }

        public IConsumerHandle StartConsuming(IConsumer consumer, IConsumerSettings settings, IChannelSettings channelSettings)
        {
            if(consumer == null)
            {
                throw new ArgumentNullException("consumer");
            }
            if(settings == null)
            {
                throw new ArgumentNullException("settings");
            }
            if(channelSettings == null)
            {
                throw new ArgumentNullException("channelSettings");
            }

            var consumerHandle = new PersistentConsumerHandle();

            persistentChannel.ChannelOpened += () => 
                consumerHandle.SetHandle(persistentChannel.StartConsuming(consumer, settings));
            
            persistentChannel.Initialise(persistentConnection, channelSettings);

            return consumerHandle;
        }

        public void Dispose()
        {
            persistentChannel.Dispose();
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
            if(consumerHandle == null)
            {
                throw new ArgumentNullException("consumerHandle");
            }

            this.consumerHandle = consumerHandle;
        }
    }
}