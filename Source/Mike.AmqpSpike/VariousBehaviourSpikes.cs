using System;
using RabbitMQ.Client;

namespace Mike.AmqpSpike
{
    public class VariousBehaviourSpikes
    {
        public void CreateModelOnClosedConnection()
        {
            var factory = new ConnectionFactory();
            var connection = factory.CreateConnection();

            connection.Close(999, "Closed by test intentionally");

            var model = connection.CreateModel();
        }

        public void ExchangeDeclarePassiveSpike()
        {
            WithChannel.Do(channel => channel.ExchangeDeclarePassive("this_does_not_exist"));
        }

        public void QueueDeclarePassiveSpike()
        {
            WithChannel.Do(channel => channel.QueueDeclarePassive("this_queue_does_not_exist"));
        }

        public void ExchangeBindWithExchangesThatDontExist()
        {
            WithChannel.Do(channel => channel.ExchangeBind("dest", "source", "some_routing_key"));
        }

        public void BasicConsumeSpike()
        {
            WithChannel.Do(channel =>
            {
                var consumerTag = channel.BasicConsume("my_queue", true, new BasicConsumer());
                Console.Out.WriteLine(consumerTag);

                channel.BasicCancel(consumerTag);
            });
        }

        
    }

    public class BasicConsumer : IBasicConsumer
    {
        public void HandleBasicConsumeOk(string consumerTag)
        {
            Console.Out.WriteLine("HandleBasicConsumeOk fired");
        }

        public void HandleBasicCancelOk(string consumerTag)
        {
            Console.Out.WriteLine("HandleBasicCancelOk fired");
        }

        public void HandleBasicCancel(string consumerTag)
        {
            Console.Out.WriteLine("HandleBasicCancel fired");
        }

        public void HandleModelShutdown(IModel model, ShutdownEventArgs reason)
        {
            Console.Out.WriteLine("HandleModelShutdown fired");
        }

        public void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            throw new System.NotImplementedException();
        }

        public IModel Model
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}