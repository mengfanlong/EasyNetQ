using System;
using System.Collections.Generic;
using EasyNetQ.AMQP;

namespace EasyNetQ.Patterns
{
    public interface IExchangeManager
    {
        bool ShouldDeclare(IExchange exchange);
        void Declared(IExchange exchange);
    }

    public class ExchangeManager : IExchangeManager
    {
        private readonly ISet<string> exchangeNames = new HashSet<string>(); 

        public bool ShouldDeclare(IExchange exchange)
        {
            if(exchange == null)
            {
                throw new ArgumentNullException("exchange");
            }

            return !exchangeNames.Contains(exchange.Name);
        }

        public void Declared(IExchange exchange)
        {
            exchangeNames.Add(exchange.Name);
        }
    }
}