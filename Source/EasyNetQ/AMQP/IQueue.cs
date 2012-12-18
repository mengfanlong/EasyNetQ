using System;

namespace EasyNetQ.AMQP
{
    public interface IQueue
    {
        string Name { get; }
        bool Durable { get; }
        bool Exclusive { get; }
        bool AutoDelete { get; }
        Arguments Arguments { get; }
        IQueue AddArgument(string key, string value);
    }

    public class Queue : IQueue
    {
        public string Name { get; private set; }
        public bool Durable { get; private set; }
        public bool Exclusive { get; private set; }
        public bool AutoDelete { get; private set; }
        public Arguments Arguments { get; private set; }

        private Queue(string name, bool durable, bool exclusive, bool autoDelete)
        {
            if(name == null)
            {
                throw new ArgumentNullException("name");
            }

            Name = name;
            Durable = durable;
            Exclusive = exclusive;
            AutoDelete = autoDelete;

            Arguments = new Arguments();
        }

        public static IQueue Create(string name)
        {
            return Create(name, new QueueSettings());
        }

        public static IQueue Create(string name, QueueSettings settings)
        {
            return new Queue(name, settings.Durable, settings.Exclusive, settings.AutoDelete);
        }

        public IQueue AddArgument(string key, string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (Arguments.ContainsKey(key))
            {
                throw new EasyNetQAmqpException("Queue already has argument '{0}'", key);
            }
            Arguments.Add(key, value);
            return this;
        }
    }

    public class QueueSettings
    {
        public bool Durable { get; set; }
        public bool Exclusive { get; set; }
        public bool AutoDelete { get; set; }

        public QueueSettings()
        {
            // defaults
            Durable = true;
            Exclusive = false;
            AutoDelete = false;
        }
    }
}