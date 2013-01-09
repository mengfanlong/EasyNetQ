namespace EasyNetQ.AMQP
{
    public interface IHandlerSelector
    {
        IHandler GetHandler(IMessageDeliveryContext messageDeliveryContext);
    }
}