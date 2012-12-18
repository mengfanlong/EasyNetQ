using System;
using RabbitMQ.Client;

namespace EasyNetQ.AMQP
{
    public interface IPersistentConnection : IDisposable
    {
        event Action Connected;
        event Action Disconnected;
        bool IsConnected { get; }
        IModel CreateModel();
        void TryToConnect();
        IChannel OpenChannel();
        IChannel OpenChannel(ChannelSettings settings);
    }
}