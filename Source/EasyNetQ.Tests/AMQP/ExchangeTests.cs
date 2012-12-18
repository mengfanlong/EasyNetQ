// ReSharper disable InconsistentNaming

using EasyNetQ.AMQP;
using NUnit.Framework;

namespace EasyNetQ.Tests.AMQP
{
    [TestFixture]
    public class ExchangeTests
    {
        private ExchangeSettings settings;

        [SetUp]
        public void SetUp()
        {
            settings = new ExchangeSettings();
        }

        [Test]
        public void Should_have_the_right_default_settings()
        {
            settings.Durable.ShouldBeTrue();
            settings.AutoDelete.ShouldBeFalse();
        }

        [Test]
        public void Should_be_able_to_create_a_direct_exchange()
        {
            var exchange = Exchange.Direct("my_exchange");
            exchange.Name.ShouldEqual("my_exchange");
            exchange.Type.ShouldEqual("direct");
            exchange.Durable.ShouldBeTrue();
            exchange.AutoDelete.ShouldBeFalse();
        }

        [Test]
        public void Should_be_able_to_create_a_topic_exchange()
        {
            var exchange = Exchange.Topic("my_exchange");
            exchange.Name.ShouldEqual("my_exchange");
            exchange.Type.ShouldEqual("topic");
            exchange.Durable.ShouldBeTrue();
            exchange.AutoDelete.ShouldBeFalse();
        }

        [Test]
        public void Should_be_able_to_create_a_fanout_exchange()
        {
            var exchange = Exchange.Fanout("my_exchange");
            exchange.Name.ShouldEqual("my_exchange");
            exchange.Type.ShouldEqual("fanout");
            exchange.Durable.ShouldBeTrue();
            exchange.AutoDelete.ShouldBeFalse();
        }

        [Test]
        public void Should_be_able_to_create_a_header_exchange()
        {
            var exchange = Exchange.Header("my_exchange");
            exchange.Name.ShouldEqual("my_exchange");
            exchange.Type.ShouldEqual("header");
            exchange.Durable.ShouldBeTrue();
            exchange.AutoDelete.ShouldBeFalse();
        }

        [Test]
        public void Should_be_able_to_create_a_custom_exchange()
        {
            settings.AutoDelete = true;
            settings.Durable = false;

            var exchange = Exchange.Custom("my_exchange", "my_custom_type", settings);
            exchange.Name.ShouldEqual("my_exchange");
            exchange.Type.ShouldEqual("my_custom_type");
            exchange.Durable.ShouldBeFalse();
            exchange.AutoDelete.ShouldBeTrue();
        }

        [Test]
        public void Should_be_able_to_add_exchange_arguments()
        {
            var exchange = Exchange.Direct("my_exchange")
                .AddArgument("key1", "value1")
                .AddArgument("key2", "value2");

            exchange.Arguments["key1"].ShouldEqual("value1");
            exchange.Arguments["key2"].ShouldEqual("value2");
        }
    }
}

// ReSharper restore InconsistentNaming