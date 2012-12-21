// ReSharper disable InconsistentNaming

using System.Text;
using EasyNetQ.AMQP;
using NUnit.Framework;
using RabbitMQ.Client;
using Rhino.Mocks;

namespace EasyNetQ.Tests.AMQP
{
    [TestFixture]
    public class PublishTests
    {
        private IModel model;
        private IChannel channel;

        [SetUp]
        public void SetUp()
        {
            model = MockRepository.GenerateStub<IModel>();
            channel = new Channel(model);

            model.Stub(x => x.CreateBasicProperties()).Return(new TestBasicProperties());
        }

        [Test]
        public void Should_be_able_to_publish_a_raw_message()
        {
            var exchange = Exchange.Direct("my_exchange");
            var settings = new PublishSettings(exchange, "my_routing_key")
            {
                Mandatory = false,
                Immediate = true
            };
            var messageBody = Encoding.UTF8.GetBytes("Hello World");
            var message = new RawMessage(messageBody);

            message.Properties.AppId.Value = "my_app_id";
            message.Properties.CorrelationId.Value = "my_correlation_id";

            channel.Publish(message, settings);

            model.AssertWasCalled(x => x.BasicPublish(
                Arg<string>.Is.Equal("my_exchange"),
                Arg<string>.Is.Equal("my_routing_key"),
                Arg<bool>.Is.Equal(false),
                Arg<bool>.Is.Equal(true),
                Arg<IBasicProperties>.Matches(properties =>
                    properties.IsAppIdPresent() &&
                    properties.AppId == "my_app_id" &&
                    properties.IsCorrelationIdPresent() &&
                    properties.CorrelationId == "my_correlation_id"
                ),
                Arg<byte[]>.Is.Equal(messageBody)));
        }
    }
}

// ReSharper restore InconsistentNaming