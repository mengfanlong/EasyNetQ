// ReSharper disable InconsistentNaming

using System.Collections.Concurrent;
using System.Threading;
using EasyNetQ.AMQP;
using EasyNetQ.Patterns;
using NUnit.Framework;
using Rhino.Mocks;

namespace EasyNetQ.Tests.Patterns
{
    [TestFixture]
    public class PublishDispatcherTests
    {
        private IPublishDispatcher publishDispatcher;
        private IChannel channel;

        [SetUp]
        public void SetUp()
        {
            channel = MockRepository.GenerateStub<IChannel>();

            publishDispatcher = new PublishDispatcher();
            publishDispatcher.Initialize(channel);
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

            channel.Stub(x => x.Publish(message, settings)).Callback<IRawMessage, IPublishSettings>((m, s) =>
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

        public void BlockingCollectionWithCancellationTokenSpike()
        {
            var source = new CancellationTokenSource();
            var queue = new BlockingCollection<int>();

            source.Cancel();
            queue.Take(source.Token);
        }
    }
}

// ReSharper restore InconsistentNaming