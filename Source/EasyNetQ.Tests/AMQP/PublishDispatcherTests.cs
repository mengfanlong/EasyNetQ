// ReSharper disable InconsistentNaming

using System.Threading;
using EasyNetQ.AMQP;
using EasyNetQ.Loggers;
using EasyNetQ.Patterns;
using NUnit.Framework;
using Rhino.Mocks;

namespace EasyNetQ.Tests.AMQP
{
    [TestFixture]
    public class PublishDispatcherTests
    {
        private IPublishDispatcher publishDispatcher;
        private IPersistentChannel persistentChannel;
        private IChannelSettings channelSettings;
        private IPersistentConnection persistentConnection;
        private IExchangeManager exchangeManager;
        private IExchange exchange;

        [SetUp]
        public void SetUp()
        {
            persistentChannel = MockRepository.GenerateStub<IPersistentChannel>();
            channelSettings = MockRepository.GenerateStub<IChannelSettings>();
            persistentConnection = MockRepository.GenerateStub<IPersistentConnection>();
            exchangeManager = MockRepository.GenerateStub<IExchangeManager>();
            exchange = MockRepository.GenerateStub<IExchange>();

            publishDispatcher = new PublishDispatcher(persistentChannel, new ConsoleLogger(), exchangeManager);
            publishDispatcher.Initialize(persistentConnection, channelSettings);
        }

        [TearDown]
        public void TearDown()
        {
            publishDispatcher.Dispose();
        }

        [Test]
        public void Should_be_able_to_publish_a_message()
        {
            var message = MockRepository.GenerateStub<IRawMessage>();
            var settings = MockRepository.GenerateStub<IPublishSettings>();
            settings.Stub(x => x.Exchange).Return(exchange);
            var reset = new AutoResetEvent(false);
            var channelPublishWasCalled = false;

            persistentChannel.Stub(x => x.Publish(message, settings)).Callback<IRawMessage, IPublishSettings>((m, s) =>
            {
                m.ShouldBeTheSameAs(message);
                s.ShouldBeTheSameAs(settings);
                channelPublishWasCalled = true;
                reset.Set();
                return true;
            });

            publishDispatcher.Publish(message, settings);

            reset.WaitOne(100);

            channelPublishWasCalled.ShouldBeTrue();
        }

        [Test]
        [ExpectedException(typeof(EasyNetQPublishException))]
        public void Should_throw_if_publish_occurs_after_dispose()
        {
            var message = MockRepository.GenerateStub<IRawMessage>();
            var settings = MockRepository.GenerateStub<IPublishSettings>();

            publishDispatcher.Dispose();

            publishDispatcher.Publish(message, settings);
        }

        [Test]
        public void Should_declare_exchange_if_instructed_by_exchange_manager()
        {
            exchangeManager.Stub(x => x.ShouldDeclare(exchange)).Return(true);

            Should_be_able_to_publish_a_message();

            persistentChannel.AssertWasCalled(x => x.Declare(exchange));
            exchangeManager.AssertWasCalled(x => x.Declared(exchange));
        }

        [Test]
        public void Should_not_declare_exchange_if_instructed_by_exchange_manager()
        {
            exchangeManager.Stub(x => x.ShouldDeclare(exchange)).Return(false);

            Should_be_able_to_publish_a_message();

            persistentChannel.AssertWasNotCalled(x => x.Declare(exchange));
            exchangeManager.AssertWasNotCalled(x => x.Declared(exchange));
        }
    }
}

// ReSharper restore InconsistentNaming