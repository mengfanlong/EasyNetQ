using System;

namespace EasyNetQ.AMQP
{
    public interface IExecutionPolicyBuilder
    {
        Action<IMessageDeliveryContext> BuildExecutionPolicy(Func<IMessageDeliveryContext, IHandler> handlerSelector);
    }
}