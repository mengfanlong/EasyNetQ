namespace EasyNetQ.AMQP
{
    public interface IHandler
    {
        void Handle(IMessageDeliveryContext messageDeliveryContext);
    }
}