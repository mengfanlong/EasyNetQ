namespace EasyNetQ.AMQP
{
    public class QueueDeletionCriteria
    {
        public bool DeleteIfUnused { get; private set; }
        public bool DeleteIfEmpty { get; private set; }

        public QueueDeletionCriteria(bool deleteIfUnused, bool deleteIfEmpty)
        {
            DeleteIfUnused = deleteIfUnused;
            DeleteIfEmpty = deleteIfEmpty;
        }
    }
}