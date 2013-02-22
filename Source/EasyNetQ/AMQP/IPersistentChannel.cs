using System;

namespace EasyNetQ.AMQP
{
    public interface IPersistentChannel : IDisposable
    {
        event Action ChannelOpened;
        event Action ChannelClosed;
        void Initialise(IPersistentConnection persistentConnection, IChannelSettings channelSettings);
        IConsumerHandle StartConsuming(IConsumer consumer, IConsumerSettings settings);
        void Publish(IRawMessage rawMessage, IPublishSettings publishSettings);
        void Declare(IExchange exchange);
    }

    public class PersistentChannel : IPersistentChannel
    {
        private IPersistentConnection persistentConnection;
        private IChannel currentChannel;
        private IChannelSettings channelSettings;

        private bool isInitialised = false;
        private bool disposed = false;

        public event Action ChannelOpened;
        public event Action ChannelClosed;

        public void OnChannelOpen()
        {
            Action handler = ChannelOpened;
            if (handler != null) handler();
        }

        public void OnChannelClosed()
        {
            Action handler = ChannelClosed;
            if (handler != null) handler();
        }

        public void Initialise(IPersistentConnection persistentConnection, IChannelSettings channelSettings)
        {
            if (disposed)
            {
                throw new EasyNetQAmqpException("PersistentChannel cannot be initialised because it is already disposed");
            }

            this.persistentConnection = persistentConnection;
            this.channelSettings = channelSettings;
            TryOpenChannel();
            isInitialised = true;
        }

        public IConsumerHandle StartConsuming(IConsumer consumer, IConsumerSettings settings)
        {
            return currentChannel.StartConsuming(consumer, settings);
        }

        public void Publish(IRawMessage rawMessage, IPublishSettings publishSettings)
        {
            currentChannel.Publish(rawMessage, publishSettings);
        }

        public void Declare(IExchange exchange)
        {
            currentChannel.Declare(exchange);
        }

        private void TryOpenChannel()
        {
            if (disposed) return;
            try
            {
                currentChannel = persistentConnection.OpenChannel(channelSettings);
                Action channelClosedHandler = null;
                channelClosedHandler = () =>
                {
                    currentChannel.ChannelClosed -= channelClosedHandler;
                    if (disposed) return;
                    currentChannel.Dispose();
                    OnChannelClosed();
                    TryOpenChannel();
                };
                currentChannel.ChannelClosed += channelClosedHandler;
                OnChannelOpen();
            }
            catch (EasyNetQOpenChannelException)
            {
                Action connectionOpenHandler = null;
                connectionOpenHandler = () =>
                {
                    persistentConnection.Connected -= connectionOpenHandler;
                    if (disposed) return;
                    TryOpenChannel();
                };
                persistentConnection.Connected += connectionOpenHandler;
            }
        }

        public void Dispose()
        {
            disposed = true;
            if (currentChannel != null)
            {
                currentChannel.Dispose();
                OnChannelClosed();
            }
        }

        private void CheckInitialisedAndNotDisposed()
        {
            if (!isInitialised)
            {
                throw new EasyNetQAmqpException("PersistentChannel is not initialised");
            }
            if (disposed)
            {
                throw new EasyNetQAmqpException("PersistentChannel is disposed");
            }
        }
    }
}