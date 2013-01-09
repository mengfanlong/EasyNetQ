using System;

namespace EasyNetQ.AMQP
{
    public class BasicHandlerSelector : IHandlerSelector
    {
        private IHandler handler = null;

        public void SetHandler(IHandler handler)
        {
            this.handler = handler;
        }

        public void SetHandler(Action<IMessageDeliveryContext> handler)
        {
            this.handler = new BasicHandler(handler);
        }

        public IHandler GetHandler(IMessageDeliveryContext messageDeliveryContext)
        {
            if(messageDeliveryContext == null)
            {
                throw new ArgumentNullException("messageDeliveryContext");
            }
            if (handler == null)
            {
                throw new EasyNetQAmqpException("A handler must be set with SetHandler before messages can be consumed");
            }

            return handler;
        }
    }

    public class BasicHandler : IHandler
    {
        private readonly Action<IMessageDeliveryContext> handler;

        public BasicHandler(Action<IMessageDeliveryContext> handler)
        {
            this.handler = handler;
        }

        public void Handle(IMessageDeliveryContext messageDeliveryContext)
        {
            handler(messageDeliveryContext);
        }
    }
}