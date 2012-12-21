// ReSharper disable InconsistentNaming

using System;
using EasyNetQ.AMQP;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing.v0_9_1;

namespace EasyNetQ.Tests.AMQP
{
    [TestFixture]
    public class PropertyConverterTests
    {
        private IBasicProperties basicProperties;

        [SetUp]
        public void SetUp()
        {
            basicProperties = new TestBasicProperties();
        }

        [Test]
        public void Should_convert_properties()
        {
            var messageProperties = new EasyNetQ.AMQP.MessageProperties
            {
                AppId = { Value = "my_app_id" },
                DeliveryMode = { Value = DeliveryMode.NonPersistent },
                MessageId = { Value = "my_message_id" }
            };

            PropertyConverter.ConvertToBasicProperties(messageProperties, basicProperties);

            basicProperties.IsAppIdPresent().ShouldBeTrue();
            basicProperties.IsDeliveryModePresent().ShouldBeTrue();
            basicProperties.IsMessageIdPresent().ShouldBeTrue();

            basicProperties.IsClusterIdPresent().ShouldBeFalse();
            basicProperties.IsContentEncodingPresent().ShouldBeFalse();
            basicProperties.IsContentTypePresent().ShouldBeFalse();
            basicProperties.IsCorrelationIdPresent().ShouldBeFalse();
            basicProperties.IsExpirationPresent().ShouldBeFalse();
            basicProperties.IsHeadersPresent().ShouldBeFalse();
            basicProperties.IsPriorityPresent().ShouldBeFalse();
            basicProperties.IsReplyToPresent().ShouldBeFalse();
            basicProperties.IsTimestampPresent().ShouldBeFalse();
            basicProperties.IsTypePresent().ShouldBeFalse();
            basicProperties.IsUserIdPresent().ShouldBeFalse();

            basicProperties.AppId.ShouldEqual("my_app_id");
            basicProperties.DeliveryMode.ShouldEqual(1);
            basicProperties.MessageId.ShouldEqual("my_message_id");
        }

        [Test]
        public void Should_convert_timestamp()
        {
            var messageProperties = new EasyNetQ.AMQP.MessageProperties
            {
                Timestamp = { Value = new DateTime(2012, 12, 19, 0, 0, 0, DateTimeKind.Utc) },
            };

            PropertyConverter.ConvertToBasicProperties(messageProperties, basicProperties);

            basicProperties.IsTimestampPresent().ShouldBeTrue();
            basicProperties.Timestamp.ShouldEqual(new AmqpTimestamp(1355875200));
        }

        [Test]
        public void Should_convert_headers()
        {
            var headers = new Headers
            {
                {"Key1", "Value1"},
                {"Key2", "Value2"}
            };

            var messageProperties = new EasyNetQ.AMQP.MessageProperties
            {
                Headers = { Value = headers }
            };

            PropertyConverter.ConvertToBasicProperties(messageProperties, basicProperties);

            basicProperties.IsHeadersPresent().ShouldBeTrue();
            basicProperties.Headers["Key1"].ShouldEqual("Value1");
            basicProperties.Headers["Key2"].ShouldEqual("Value2");
        }
    }

    public class TestBasicProperties : BasicProperties
    {
        
    }
}

// ReSharper restore InconsistentNaming