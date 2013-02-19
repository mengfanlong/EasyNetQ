// ReSharper disable InconsistentNaming

using System.Text;
using EasyNetQ.AMQP;
using EasyNetQ.Patterns;
using NUnit.Framework;
using Rhino.Mocks;

namespace EasyNetQ.Tests.Patterns
{
    [TestFixture]
    public class ConsumerPipelineBuilderTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void Should_transform_message_correctly()
        {
            var messageBody = Encoding.UTF8.GetBytes("{ Text: \"Hello World\"}");
            var context = MockRepository.GenerateStub<IMessageDeliveryContext>();
            context.Stub(x => x.Body).Return(messageBody);

            IConsumerPipelineBuilder pipelineBuilder = new ConsumerPipelineBuilder();

            var message = pipelineBuilder.CreatePipeline<MyMessage>()(context);

            message.Text.ShouldEqual("Hello World");
        }
    }
}

// ReSharper restore InconsistentNaming