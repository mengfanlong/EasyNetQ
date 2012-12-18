using System;

namespace EasyNetQ.AMQP
{
    public class QueueDeclareResult
    {
        public uint ConsumerCount { get; private set; }
        public uint MessageCount { get; private set; }
        public string QueueName { get; private set; }
        public bool QueueExists { get; private set; }

        public QueueDeclareResult(uint consumerCount, uint messageCount, string queueName, bool queueExists)
        {
            QueueExists = queueExists;
            if(queueName == null)
            {
                throw new ArgumentNullException("queueName");
            }

            ConsumerCount = consumerCount;
            MessageCount = messageCount;
            QueueName = queueName;
        }
    }
}