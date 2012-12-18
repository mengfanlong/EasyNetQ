namespace EasyNetQ.AMQP
{
    public class QueuePurgeResult
    {
        public uint NumberOfMessagesPurged { get; private set; }

        public QueuePurgeResult(uint numberOfMessagesPurged)
        {
            NumberOfMessagesPurged = numberOfMessagesPurged;
        }
    }
}