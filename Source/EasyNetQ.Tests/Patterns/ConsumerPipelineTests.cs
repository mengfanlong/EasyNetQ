// ReSharper disable InconsistentNaming

using System;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json;

namespace EasyNetQ.Tests.Patterns
{
    [TestFixture]
    public class ConsumerPipelineTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void Should_transform_message_correctly()
        {
            var messageBody = Encoding.UTF8.GetBytes("{ Text: \"Hello World\"}");
            var context = new ConsumerContext(messageBody);

            var pipeline =
                from s in PipelineElements.ConvertToString()
                from t in PipelineElements.DeSerialize<MyMessage>(s)
                select t;

            var message = pipeline(context);

            message.Text.ShouldEqual("Hello World");
        }
    }

    public static class PipelineElements
    {
        public static ConsumerTransformer<string> ConvertToString()
        {
            return context => Encoding.UTF8.GetString(context.Body);
        }

        public static ConsumerTransformer<T> DeSerialize<T>(string body)
        {
            return context => JsonConvert.DeserializeObject<T>(body);
        } 
    }

    public delegate T ConsumerTransformer<T>(ConsumerContext context);

    public class ConsumerContext
    {
        public byte[] Body { get; private set; }

        public ConsumerContext(byte[] body)
        {
            Body = body;
        }
    }

    public static class ConsumerTransformerExtensions
    {
        public static ConsumerTransformer<C> SelectMany<A, B, C>(
            this ConsumerTransformer<A> source,
            Func<A, ConsumerTransformer<B>> transformer,
            Func<A,B,C> select)
        {
            return context =>
            {
                var a = source(context);
                var b = transformer(a)(context);
                return select(a, b);
            };
        }
    }
}

// ReSharper restore InconsistentNaming