// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Newtonsoft.Json;

namespace EasyNetQ.Tests.Patterns
{
    [TestFixture]
    public class MessagePipelineSpike
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void Should_be_able_to_build_a_message_pipeline()
        {
            var bus = new FakeBus();

            bus.Subscribe<MyMessage>(x => Console.WriteLine("Got MyMessage: '{0}'", x.Text));
            bus.Subscribe<MyOtherMessage>(x => Console.WriteLine("Got MyOtherMessage: '{0}'", x.Text));

            bus.Publish(new MyOtherMessage { Text = "Dogs!"});
            bus.Publish(new MyMessage{ Text = "pigs!"});
            bus.Publish(new MyMessage{ Text = "sheep!"});

            Thread.Sleep(100);
            bus.Dispose();
        }
    }

    public class FakeBus : IDisposable
    {
        private readonly BlockingCollection<RawMessage> queue = new BlockingCollection<RawMessage>();
        private readonly ConcurrentDictionary<string, Action<RawMessage>> handlers =
            new ConcurrentDictionary<string, Action<RawMessage>>(); 

        public FakeBus()
        {
            var consumerThread = new Thread(() =>
            {
                while (!disposed)
                {
                    var raw = queue.Take();
                    if (!handlers.ContainsKey(raw.MessageType))
                    {
                        Console.Out.WriteLine("Could find handler for message type: '{0}'", raw.MessageType);
                    }
                    else
                    {
                        handlers[raw.MessageType](raw);
                    }
                }
            });
            consumerThread.Start();
        }

        public void Publish<TMessage>(TMessage message)
        {
            queue.Add(GetProducerPipeline<TMessage>()(message));
        }

        public void Subscribe<TMessage>(Action<TMessage> handler)
        {
            handlers.TryAdd(typeof (TMessage).Name, raw => handler(GetConsumerPipeline<TMessage>()(raw)));
        }

        public Func<TMessage, RawMessage> GetProducerPipeline<TMessage>()
        {
            return message =>
            {
                var json = JsonConvert.SerializeObject(message);
                var bytes = Encoding.UTF8.GetBytes(json);
                return new RawMessage(typeof(TMessage).Name, bytes);
            };
        }

        public Func<RawMessage, TMessage> GetConsumerPipeline<TMessage>()
        {
            return raw =>
            {
                var pipeline = 
                    from s in PipelineSteps.GetStringValue()
                    from t in PipelineSteps.Deserialize<TMessage>(s)
                    select t;

                return pipeline(raw);
            };
        }

        private bool disposed = false;
        public void Dispose()
        {
            disposed = true;
        }
    }

    public static class PipelineSteps
    {
        public static PipelineTransformer<string> GetStringValue()
        {
            return raw => Encoding.UTF8.GetString(raw.Body);
        }

        public static PipelineTransformer<T> Deserialize<T>(string json)
        {
            return raw => JsonConvert.DeserializeObject<T>(json);
        } 
    }

    public delegate T PipelineTransformer<T>(RawMessage raw); 

    public class PipelineResult<T>
    {
        public T Value { get; private set; }
        public RawMessage RawMessage { get; private set; }

        public PipelineResult(T value, RawMessage rawMessage)
        {
            Value = value;
            RawMessage = rawMessage;
        }
    }

    public static class PipelineResultExtensions
    {
        public static PipelineTransformer<C> SelectMany<A, B, C>(
            this PipelineTransformer<A> source,
            Func<A, PipelineTransformer<B>> transform,
            Func<A,B,C> select 
            )
        {
            return raw =>
            {
                var a = source(raw);
                var b = transform(a)(raw);
                return select(a, b);
            };
        }
    }

    public class RawMessage
    {
        public string MessageType { get; private set; }
        public byte[] Body { get; private set; }

        public RawMessage(string messageType, byte[] body)
        {
            MessageType = messageType;
            Body = body;
        }
    }
}

// ReSharper restore InconsistentNaming