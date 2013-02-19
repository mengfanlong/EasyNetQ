// ReSharper disable InconsistentNaming

using System.Threading;
using EasyNetQ.AMQP;
using EasyNetQ.Loggers;
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

        [SetUp]
        public void SetUp()
        {
            persistentChannel = MockRepository.GenerateStub<IPersistentChannel>();
            channelSettings = MockRepository.GenerateStub<IChannelSettings>();
            persistentConnection = MockRepository.GenerateStub<IPersistentConnection>();

            publishDispatcher = new PublishDispatcher(persistentChannel, new ConsoleLogger());
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
    }
}

// ReSharper restore InconsistentNaming