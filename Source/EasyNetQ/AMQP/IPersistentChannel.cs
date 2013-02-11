using System;

namespace EasyNetQ.AMQP
{
    public interface IPersistentChannel : IDisposable
    {
        event Action ChannelOpened;
        event Action ChannelClosed;
        void Initialise(IPersistentConnection persistentConnection, IChannelSettings channelSettings);
        IConsumerHandle StartConsuming(IConsumer consumer, IConsumerSettings settings);
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
            this.persistentConnection = persistentConnection;
            this.channelSettings = channelSettings;
            TryOpenChannel();
            isInitialised = true;
        }

        public IConsumerHandle StartConsuming(IConsumer consumer, IConsumerSettings settings)
        {
            return currentChannel.StartConsuming(consumer, settings);
        }

        private void TryOpenChannel()
        {
            try
            {
                currentChannel = persistentConnection.OpenChannel(channelSettings);
                Action channelClosedHandler = null;
                channelClosedHandler = () =>
                {
                    currentChannel.ChannelClosed -= channelClosedHandler;
                    currentChannel.Dispose();
                    OnChannelClosed();
                    TryOpenChannel();
                };
                currentChannel.ChannelClosed += channelClosedHandler;
                OnChannelOpen();
            }
            catch (Exception)
            {
                Action connectionOpenHandler = null;
                connectionOpenHandler = () =>
                {
                    persistentConnection.Connected -= connectionOpenHandler;
                    TryOpenChannel();
                };
                persistentConnection.Connected += connectionOpenHandler;
            }
        }

        public void Dispose()
        {
            if(currentChannel != null)
            {
                currentChannel.Dispose();
                OnChannelClosed();
            }
            disposed = true;
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