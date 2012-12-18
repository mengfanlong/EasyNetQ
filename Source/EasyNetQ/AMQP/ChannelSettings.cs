namespace EasyNetQ.AMQP
{
    public class ChannelSettings
    {
        public ushort PrefetchCount { get; set; }
        public bool PublisherConfirmsOn { get; set; }

        public ChannelSettings()
        {
            // default settings
            PrefetchCount = 50;
            PublisherConfirmsOn = false;
        }
    }
}