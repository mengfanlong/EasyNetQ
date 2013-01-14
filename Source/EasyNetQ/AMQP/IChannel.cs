using System;

namespace EasyNetQ.AMQP
{
    public interface IChannel : IDisposable
    {
        event Action ChannelClosed;

        void Declare(IExchange exchange);
        bool Exists(IExchange exchange);
        void Delete(IExchange exchange);
        QueueDeclareResult Declare(IQueue queue);
        IQueue DeclareBrokerGeneratedQueue();
        QueueDeclareResult Exists(IQueue queue);
        QueuePurgeResult Delete(IQueue queue);
        QueuePurgeResult Delete(IQueue queue, QueueDeletionCriteria criteria);
        QueuePurgeResult Purge(IQueue queue);
        void Bind(IExchange source, IExchange destination, string routingKey);
        void Bind(IExchange exchange, IQueue queue, string routingKey);
        void Bind(IExchange exchange, IQueue queue, string routingKey, Arguments arguments);
        void UnBind(IExchange source, IExchange destination, string routingKey);
        void UnBind(IExchange exchange, IQueue queue, string routingKey, Arguments arguments);
        void Publish(IRawMessage message, PublishSettings settings);
        void StartConsuming(IConsumer consumer, ConsumerSettings settings);
        void Acknowledge(ulong deliveryTag, bool multiple);
        void Reject(ulong deliveryTag, bool requeue);
    }
}