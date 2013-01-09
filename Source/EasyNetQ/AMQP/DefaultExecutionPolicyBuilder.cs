using System;

namespace EasyNetQ.AMQP
{
    public class DefaultExecutionPolicyBuilder : IExecutionPolicyBuilder
    {
        public Action<IMessageDeliveryContext> BuildExecutionPolicy(Func<IMessageDeliveryContext, IHandler> handlerSelector)
        {
            return context =>
            {
                try
                {
                    var handler = handlerSelector(context);
                    handler.Handle(context);
                    context.Channel.Acknowledge(context.DeliveryTag, false);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            };
        }
    }
}