// ReSharper disable InconsistentNaming

using System;
using System.Text;
using EasyNetQ.Patterns;
using NUnit.Framework;

namespace EasyNetQ.Tests.Patterns
{
    [TestFixture]
    public class ProducerPipelineBuilderTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void Should_transform_message_correctly()
        {
            IProducerPipelineBuilder pipelineBuilder = new ProducerPipelineBuilder();
            
            var message = new MyMessage { Text = "Hello World" };

            var pipeline = pipelineBuilder.CreatePipeline(message);

            var context = new MessagePublishingContext();
            pipeline(context);

            var messageBodyBytes = context.RawMessage.Body;
            var messageBodyString = Encoding.UTF8.GetString(messageBodyBytes);

            messageBodyString.ShouldEqual("{\"Text\":\"Hello World\"}");
            context.RawMessage.Properties.Type.Value.ShouldEqual("MyMessage");
            context.RawMessage.Properties.ContentType.Value.ShouldEqual("application/json");
            context.RawMessage.Properties.ContentEncoding.Value.ShouldEqual("UTF8");

            Guid.Parse(context.RawMessage.Properties.CorrelationId.Value);
        }
    }
}

// ReSharper restore InconsistentNaming