using System;

namespace EasyNetQ.AMQP
{
    public class ConsumerSettings
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