// ReSharper disable InconsistentNaming

using System;
using EasyNetQ.AMQP;
using EasyNetQ.Tests.Mocking;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Rhino.Mocks;

namespace EasyNetQ.Tests.AMQP
{
    [TestFixture]
    public class PersistentConnectionTests
    {
        private IPersistentConnection persistentConnection;
        private IConnectionFactory connectionFactory;
        private IConnection connection;
        private IConnectionRetryTimer connectionRetryTimer;
        private RecordingLogger recordingLogger;
        private IChannelFactory channelFactory;

        [SetUp]
        public void SetUp()
        {
            connection = MockRepository.GenerateStub<IConnection>();
            connection.Stub(x => x.IsOpen).Return(true);

            connectionFactory = MockRepository.GenerateStub<IConnectionFactory>();
            connectionFactory.Stub(x => x.CurrentHost).Return(new HostConfiguration
            {
                Host = "localhost",
                Port = 1234
            });
            connectionFactory.Stub(x => x.Configuration).Return(new ConnectionConfiguration
            {
                VirtualHost = "/"
            });
            connectionRetryTimer = MockRepository.GenerateStub<IConnectionRetryTimer>();
            recordingLogger = new RecordingLogger{ SurpressConsoleOutput = true };
            channelFactory = MockRepository.GenerateStub<IChannelFactory>();

            persistentConnection = new PersistentConnection(
                connectionFactory, 
                recordingLogger, 
                connectionRetryTimer,
                channelFactory);
        }

        [Test]
        public void Should_connect_successfully()
        {
            connectionFactory.Stub(x => x.CreateConnection()).Return(connection);
            connectionFactory.Stub(x => x.Next()).Return(false);
            connectionFactory.Stub(x => x.Succeeded).Return(true);

            persistentConnection.TryToConnect();

            connectionFactory.AssertWasCalled(x => x.Success());

            const string expectedLogMessage =
@"DEBUG: Trying to connect
DEBUG: OnConnected event fired
INFO: Connected to RabbitMQ. Broker: 'localhost', Port: 1234, VHost: '/'
";
            recordingLogger.LogMessages.ShouldEqual(expectedLogMessage);
        }

        [Test]
        public void Should_fire_connected_event_when_connected_successfully()
        {
            var connectedEventFired = false;
            persistentConnection.Connected += () => connectedEventFired = true;

            Should_connect_successfully();

            connectedEventFired.ShouldBeTrue();
        }

        [Test]
        public void Should_return_true_from_IsConnected_when_connected_successfully()
        {
            Should_connect_successfully();
            persistentConnection.IsConnected.ShouldBeTrue();
        }

        [Test]
        public void Should_start_retry_timer_on_socket_exception()
        {
            connectionFactory.Stub(x => x.CreateConnection()).Throw(new System.Net.Sockets.SocketException());
            connectionFactory.Stub(x => x.Next()).Return(false);
            connectionFactory.Stub(x => x.Succeeded).Return(false);
            connectionRetryTimer.Stub(x => x.RetryIntervalSeconds).Return(66);

            persistentConnection.TryToConnect();

            connectionFactory.AssertWasNotCalled(x => x.Success());
            connectionRetryTimer.AssertWasCalled(x => x.ReTry(Arg<Action>.Is.Anything));

            const string expectedLogMessage =
@"DEBUG: Trying to connect
ERROR: Failed to connect to Broker: 'localhost', Port: 1234 VHost: '/'. ExceptionMessage: 'The operation completed successfully'
ERROR: Failed to connected to any Broker. Retrying in 66 seconds
";
            recordingLogger.LogMessages.ShouldEqual(expectedLogMessage);
        }

        [Test]
        public void Should_start_retry_timer_on_BrokerUnreachableException()
        {
            connectionFactory.Stub(x => x.CreateConnection()).Throw(new BrokerUnreachableException(null, null, null));
            connectionFactory.Stub(x => x.Next()).Return(false);
            connectionFactory.Stub(x => x.Succeeded).Return(false);
            connectionRetryTimer.Stub(x => x.RetryIntervalSeconds).Return(66);

            persistentConnection.TryToConnect();

            connectionFactory.AssertWasNotCalled(x => x.Success());
            connectionRetryTimer.AssertWasCalled(x => x.ReTry(Arg<Action>.Is.Anything));

            const string expectedLogMessage =
@"DEBUG: Trying to connect
ERROR: Failed to connect to Broker: 'localhost', Port: 1234 VHost: '/'. ExceptionMessage: 'None of the specified endpoints were reachable'
ERROR: Failed to connected to any Broker. Retrying in 66 seconds
";
            recordingLogger.LogMessages.ShouldEqual(expectedLogMessage);
        }

        [Test]
        public void Should_not_attempt_to_connect_if_disposed()
        {
            persistentConnection.Dispose();
            persistentConnection.TryToConnect();

            connectionFactory.AssertWasNotCalled(x => x.CreateConnection());

            const string expectedLogMessage =
@"DEBUG: Trying to connect
ERROR: Cannot connect a disposed connection
";
            recordingLogger.LogMessages.ShouldEqual(expectedLogMessage);
        }

        [Test]
        public void Should_reconnect_after_connection_shutdown()
        {
            var disconnectFired = false;
            persistentConnection.Disconnected += () => disconnectFired = true;

            Should_connect_successfully();

            var shutdownEventArgs = new ShutdownEventArgs(ShutdownInitiator.Peer, 0, "the reason for shutdown");

            connection.Raise(x => x.ConnectionShutdown += null, connection, shutdownEventArgs);

            disconnectFired.ShouldBeTrue();

            const string expectedLogMessage = 
@"DEBUG: Trying to connect
DEBUG: OnConnected event fired
INFO: Connected to RabbitMQ. Broker: 'localhost', Port: 1234, VHost: '/'
INFO: Disconnected from RabbitMQ Broker. Connection reset by Peer. Reason: 'the reason for shutdown'
DEBUG: Trying to connect
DEBUG: OnConnected event fired
INFO: Connected to RabbitMQ. Broker: 'localhost', Port: 1234, VHost: '/'
";
            recordingLogger.LogMessages.ShouldEqual(expectedLogMessage);
        }

        [Test]
        public void Should_create_new_channel()
        {
            Should_connect_successfully();

            var settings = new ChannelSettings();
            var expectedChannel = MockRepository.GenerateStub<IChannel>();
            channelFactory.Stub(x => x.OpenChannel(connection, settings)).Return(expectedChannel);

            var channel = persistentConnection.OpenChannel(settings);

            channel.ShouldBeTheSameAs(expectedChannel);
        }
    }
}

// ReSharper restore InconsistentNaming