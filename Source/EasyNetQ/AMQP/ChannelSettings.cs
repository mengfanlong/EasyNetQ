namespace EasyNetQ.AMQP
{
    public interface IChannelSettings
    {
        ushort PrefetchCount { get; }
        bool PublisherConfirmsOn { get; }
    }

    public class ChannelSettings : IChannelSettings
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