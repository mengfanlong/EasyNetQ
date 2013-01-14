using System;

namespace EasyNetQ.AMQP
{
    public class PersistentConsumer : IPersistentConsumer
    {
        private readonly IPersistentConnection connection;

        public PersistentConsumer(IPersistentConnection connection)
        {
            this.connection = connection;
        }

        public void StartConsuming(IConsumer consumer, ConsumerSettings settings, ChannelSettings channelSettings)
        {
            try
            {
                var channel = connection.OpenChannel(channelSettings);
                Action channelClosedHandler = null;
                channelClosedHandler = () =>
                {
                    channel.ChannelClosed -= channelClosedHandler;
                    channel.Dispose();
                    StartConsuming(consumer, settings, channelSettings);
                };
                channel.ChannelClosed += channelClosedHandler;

                channel.StartConsuming(consumer, settings);
            }
            catch (RabbitMQ.Client.Exceptions.OperationInterruptedException)
            {
                Action connectionOpenHandler = null;
                connectionOpenHandler = () =>
                {
                    connection.Connected -= connectionOpenHandler;
                    StartConsuming(consumer, settings, channelSettings);
                };
                connection.Connected += connectionOpenHandler;
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}