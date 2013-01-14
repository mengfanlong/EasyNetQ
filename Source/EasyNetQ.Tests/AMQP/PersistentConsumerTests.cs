// ReSharper disable InconsistentNaming

using EasyNetQ.AMQP;
using NUnit.Framework;
using RabbitMQ.Client;
using Rhino.Mocks;

namespace EasyNetQ.Tests.AMQP
{
    [TestFixture]
    public class PersistentConsumerTests
    {
        private IConsumer consumer;
        private IPersistentConnection connection;
        private IChannel channel;
        private IPersistentConsumer persistentConsumer;

        private readonly ConsumerSettings settings = new ConsumerSettings(Queue.Create("my_queue"));
        private readonly ChannelSettings channelSettings = new ChannelSettings();

        [SetUp]
        public void SetUp()
        {
            consumer = MockRepository.GenerateStub<IConsumer>();
            connection = MockRepository.GenerateStub<IPersistentConnection>();
            channel = MockRepository.GenerateStub<IChannel>();

            persistentConsumer = new PersistentConsumer(connection);
        }

        [Test]
        public void Should_start_consuming_when_persistenConnection_starts_consuming()
        {
            connection.Stub(x => x.OpenChannel(Arg<ChannelSettings>.Is.Anything)).Return(channel);
            
            persistentConsumer.StartConsuming(consumer, settings, channelSettings);
            channel.AssertWasCalled(x => x.StartConsuming(consumer, settings));
        }

        [Test]
        public void Should_restart_consumer_on_new_channel_when_connection_is_closed()
        {
            connection.Stub(x => x.OpenChannel(Arg<ChannelSettings>.Is.Anything)).Return(channel);

            var numberOfStartConsumingCalls = 0;
            channel.Stub(x => x.StartConsuming(consumer, settings))
                .Callback<IConsumer, ConsumerSettings>((c, s) =>
                {
                    numberOfStartConsumingCalls++;
                    return true;
                });

            persistentConsumer.StartConsuming(consumer, settings, channelSettings);

            numberOfStartConsumingCalls.ShouldEqual(1);
            channel.Raise(x => x.ChannelClosed += null);
            numberOfStartConsumingCalls.ShouldEqual(2);
        }

        [Test]
        public void Should_retry_opening_channel_if_connection_is_closed()
        {
            var isFirstCall = true;

            connection.Stub(x => x.OpenChannel(Arg<ChannelSettings>.Is.Anything))
                .Callback<ChannelSettings>(s =>
                {
                    if (isFirstCall)
                    {
                        isFirstCall = false;
                        throw new RabbitMQ.Client.Exceptions.OperationInterruptedException(
                            new ShutdownEventArgs(ShutdownInitiator.Peer, 0, ""));
                    }
                    return true;
                })
                .Return(channel);

            persistentConsumer.StartConsuming(consumer, settings, channelSettings);

            connection.Raise(x => x.Connected += null);

            channel.AssertWasCalled(x => x.StartConsuming(consumer, settings));
        }

        [Test]
        public void Should_dispose_open_channels_and_disconnect_event_handlers_on_dispose()
        {
            // TODO
            persistentConsumer.Dispose();
        }
    }
}

// ReSharper restore InconsistentNaming