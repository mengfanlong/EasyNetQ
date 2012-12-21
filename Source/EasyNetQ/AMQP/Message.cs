namespace EasyNetQ.AMQP
{
    public interface IMessage
    {
        IMessageProperties Properties { get; }
    }

    public interface IRawMessage : IMessage
    {
        byte[] Body { get; }
    }

    public interface ITypedMessage<T> : IMessage
    {
        T Value { get; }
    }

    public class RawMessage : IRawMessage
    {
        public RawMessage(byte[] body)
        {
            Body = body;
            Properties = new MessageProperties();
        }

        public IMessageProperties Properties { get; private set; }
        public byte[] Body { get; private set; }
    }

    public class TypedMessage<T> : ITypedMessage<T>
    {
        public TypedMessage(T value)
        {
            Value = value;
            Properties = new MessageProperties();
        }

        public IMessageProperties Properties { get; private set; }
        public T Value { get; private set; }
    }
}