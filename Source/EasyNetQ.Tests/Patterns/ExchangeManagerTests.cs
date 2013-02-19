// ReSharper disable InconsistentNaming

using EasyNetQ.AMQP;
using EasyNetQ.Patterns;
using NUnit.Framework;

namespace EasyNetQ.Tests.Patterns
{
    [TestFixture]
    public class ExchangeManagerTests
    {
        private IExchangeManager exchangeManager;

        [SetUp]
        public void SetUp()
        {
            exchangeManager = new ExchangeManager();
        }

        [Test]
        public void Should_declare_an_exchange_the_first_time_its_seen()
        {
            var exchange = Exchange.Direct("this.is.unique", new ExchangeSettings
            {
                AutoDelete = false,
                Durable = true
            });

            exchangeManager.ShouldDeclare(exchange).ShouldBeTrue();
        }

        [Test]
        public void Should_not_declare_exchange_the_second_time_its_seen()
        {
            var exchange = Exchange.Direct("this.is.unique", new ExchangeSettings
            {
                AutoDelete = false,
                Durable = true
            });

            exchangeManager.Declared(exchange);
            exchangeManager.ShouldDeclare(exchange).ShouldBeFalse();
        }
    }
}

// ReSharper restore InconsistentNaming