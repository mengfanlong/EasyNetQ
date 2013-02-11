using System;

namespace EasyNetQ.AMQP
{
    public interface IConsumerSettings
    {
        IQueue Queue { get; }
        string ConsumerTag { get; }
        bool NoAck { get; }
        bool Exclusive { get; }
        Arguments Arguments { get; }
    }

    public class ConsumerSettings : IConsumerSettings
    {
        public IQueue Queue { get; private set; }
        public string ConsumerTag { get; set; }
        public bool NoAck { get; set; }
        public bool Exclusive { get; set; }
        public Arguments Arguments { get; set; }

        public ConsumerSettings(IQueue queue)
        {
            if(queue == null)
            {
                throw new ArgumentNullException("queue");
            }

            Queue = queue;
            Arguments = new Arguments();
        }
    }
}