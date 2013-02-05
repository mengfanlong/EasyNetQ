using RabbitMQ.Client;

namespace EasyNetQ.AMQP
{
    public class ConsumerHandle : IConsumerHandle
    {
        private readonly string consumerTag;
        private readonly IModel model;

        public ConsumerHandle(string consumerTag, IModel model)
        {
            this.consumerTag = consumerTag;
            this.model = model;
        }

        public void Dispose()
        {
            model.BasicCancel(consumerTag);
        }
    }
}