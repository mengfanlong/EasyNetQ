// ReSharper disable InconsistentNaming

using EasyNetQ.AMQP;
using EasyNetQ.Loggers;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Rhino.Mocks;

namespace EasyNetQ.Tests.AMQP
{
    [TestFixture]
    public class ChannelFactoryTests
    {
        private IChannelFactory channelFactory;
        private IModel model;
        private IConnection connection;

        [SetUp]
        public void SetUp()
        {
            model = MockRepository.GenerateStub<IModel>();
            connection = MockRepository.GenerateStub<IConnection>();

            channelFactory = new ChannelFactory(new ConsoleLogger());
        }

        [Test]
        public void Should_create_new_model_on_open_channel()
        {
            connection.Stub(x => x.CreateModel()).Return(model);
            var settings = new ChannelSettings();

            channelFactory.OpenChannel(connection, settings);

            connection.AssertWasCalled(x => x.CreateModel());
        }

        [Test]
        public void Should_set_publisher_confirms_set_on_settings()
        {
            connection.Stub(x => x.CreateModel()).Return(model);
            var settings = new ChannelSettings
            {
                PublisherConfirmsOn = true
            };

            channelFactory.OpenChannel(connection, settings);

            connection.AssertWasCalled(x => x.CreateModel());
            model.AssertWasCalled(x => x.ConfirmSelect());
        }

        [Test]
        public void Should_set_prefetch_count()
        {
            connection.Stub(x => x.CreateModel()).Return(model);
            var settings = new ChannelSettings
            {
                PrefetchCount = 34
            };

            channelFactory.OpenChannel(connection, settings);

            connection.AssertWasCalled(x => x.CreateModel());
            model.AssertWasCalled(x => x.BasicQos(0, 34, false));
        }

        [Test]
        [ExpectedException(typeof(EasyNetQAmqpException))]
        public void Should_thow_easyNetQ_exception_if_model_cannot_be_created()
        {
            var shutdownEventArgs = new ShutdownEventArgs(ShutdownInitiator.Peer, 0, "connection lost");
            connection.Stub(x => x.CreateModel()).Throw(new OperationInterruptedException(shutdownEventArgs));
            var settings = new ChannelSettings();

            channelFactory.OpenChannel(connection, settings);
        }
    }
}

// ReSharper restore InconsistentNaming