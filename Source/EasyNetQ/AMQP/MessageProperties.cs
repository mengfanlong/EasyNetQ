using System;
using System.Runtime.Serialization;

namespace EasyNetQ.AMQP
{
    public interface IMessageProperties
    {
        PropertyValue<string> AppId { get; }
        PropertyValue<string> ClusterId { get; }
        PropertyValue<string> ContentEncoding { get; }
        PropertyValue<string> ContentType { get; }
        PropertyValue<string> CorrelationId { get; }
        PropertyValue<DeliveryMode> DeliveryMode { get; }
        PropertyValue<string> Expiration { get; }
        PropertyValue<Headers> Headers { get; }
        PropertyValue<string> MessageId { get; }
        PropertyValue<byte> Priority { get; }
        PropertyValue<string> ReplyTo { get; }
        PropertyValue<DateTime> Timestamp { get; }
        PropertyValue<string> Type { get; }
        PropertyValue<string> UserId { get; }
    }

    public class MessageProperties : IMessageProperties
    {
        public PropertyValue<string>        AppId { get; private set; }
        public PropertyValue<string>        ClusterId { get; private set; }
        public PropertyValue<string>        ContentEncoding { get; private set; }
        public PropertyValue<string>        ContentType { get; private set; }
        public PropertyValue<string>        CorrelationId { get; private set; }
        public PropertyValue<DeliveryMode>  DeliveryMode { get; private set; }
        public PropertyValue<string>        Expiration { get; private set; }
        public PropertyValue<Headers>       Headers { get; private set; }
        public PropertyValue<string>        MessageId { get; private set; }
        public PropertyValue<byte>          Priority { get; private set; }
        public PropertyValue<string>        ReplyTo { get; private set; }
        public PropertyValue<DateTime>      Timestamp { get; private set; }
        public PropertyValue<string>        Type { get; private set; }
        public PropertyValue<string>        UserId { get; private set; }

        public MessageProperties()
        {
            AppId = new PropertyValue<string>();
            ClusterId = new PropertyValue<string>();
            ContentEncoding = new PropertyValue<string>();
            ContentType = new PropertyValue<string>();
            CorrelationId = new PropertyValue<string>();
            DeliveryMode = new PropertyValue<DeliveryMode>();
            Expiration = new PropertyValue<string>();
            Headers = new PropertyValue<Headers>();
            MessageId = new PropertyValue<string>();
            Priority = new PropertyValue<byte>();
            ReplyTo = new PropertyValue<string>();
            Timestamp = new PropertyValue<DateTime>();
            Type = new PropertyValue<string>();
            UserId = new PropertyValue<string>();
        }
    }

    public enum DeliveryMode
    {
        NonPersistent = 1,
        Persistent = 2
    }

    public interface IPropertyValue
    {
        bool IsSet { get; }
        void Clear();
        object GetValue();
    }

    public class PropertyValue<T> : IPropertyValue
    {
        public bool IsSet { get; private set; }
        private T value;

        public void Clear()
        {
            IsSet = false;
        }

        public object GetValue()
        {
            return Value;
        }

        public T Value
        {
            get
            {
                if(!IsSet) throw new PropertyNotSetException();
                return value;
            }
            set
            {
                this.value = value;
                IsSet = true;
            }
        }
    }

    [Serializable]
    public class PropertyNotSetException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public PropertyNotSetException()
        {
        }

        public PropertyNotSetException(string message, Exception inner) : base(message, inner)
        {
        }

        protected PropertyNotSetException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}