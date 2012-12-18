using System;
using System.Collections.Generic;

namespace EasyNetQ.AMQP
{
    public interface IExchange
    {
        string Name { get; }
        string Type { get; }
        bool Durable { get; }
        bool AutoDelete { get; }
        Arguments Arguments { get; }
        IExchange AddArgument(string key, string value);
    }

    public class Exchange : IExchange
    {
        public string Name { get; private set; }
        public string Type { get; private set; }
        public bool Durable { get; private set; }
        public bool AutoDelete { get; private set; }
        public Arguments Arguments { get; private set; }

        private static readonly ISet<string> exchangeTypes = new HashSet<string>{ "direct", "topic", "fanout", "header" }; 

        private Exchange(string name, string type, bool durable, bool autoDelete)
        {
            if(name == null)
            {
                throw new ArgumentNullException("name");
            }
            if(type == null)
            {
                throw new ArgumentNullException("type");
            }

            AutoDelete = autoDelete;
            Durable = durable;
            Type = type;
            Name = name;
            Arguments = new Arguments();
        }

        public static IExchange Default()
        {
            return Direct("amqp.default");
        }

        public static IExchange Direct(string name)
        {
            return Direct(name, new ExchangeSettings());
        }

        public static IExchange Direct(string name, ExchangeSettings settings)
        {
            return new Exchange(name, "direct", settings.Durable, settings.AutoDelete);
        }

        public static IExchange Topic(string name)
        {
            return Topic(name, new ExchangeSettings());
        }

        public static IExchange Topic(string name, ExchangeSettings settings)
        {
            return new Exchange(name, "topic", settings.Durable, settings.AutoDelete);
        }

        public static IExchange Fanout(string name)
        {
            return Fanout(name, new ExchangeSettings());
        }

        public static IExchange Fanout(string name, ExchangeSettings settings)
        {
            return new Exchange(name, "fanout", settings.Durable, settings.AutoDelete);
        }

        public static IExchange Header(string name)
        {
            return Header(name, new ExchangeSettings());
        }

        public static IExchange Header(string name, ExchangeSettings settings)
        {
            return new Exchange(name, "header", settings.Durable, settings.AutoDelete);
        }

        public static IExchange Custom(string name, string customExchangeTypeName, ExchangeSettings settings)
        {
            if(customExchangeTypeName == null)
            {
                throw new ArgumentNullException("customExchangeTypeName");
            }
            if (exchangeTypes.Contains(customExchangeTypeName))
            {
                throw new ArgumentException(string.Format(
                    "customExchangeTypeName cannot be one of {0}. Please use the provided static methods instead.", 
                    string.Join(", ", exchangeTypes)));
            }

            return new Exchange(name, customExchangeTypeName, settings.Durable, settings.AutoDelete);
        }

        public IExchange AddArgument(string key, string value)
        {
            if(key == null)
            {
                throw new ArgumentNullException("key");
            }
            if(value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (Arguments.ContainsKey(key))
            {
                throw new EasyNetQAmqpException("Exchange already has argument '{0}'", key);
            }
            Arguments.Add(key, value);
            return this;
        }
    }

    public class ExchangeSettings
    {
        public bool Durable { get; set; }
        public bool AutoDelete { get; set; }

        public ExchangeSettings()
        {
            // default settings
            Durable = true;
            AutoDelete = false;
        }
    }
}