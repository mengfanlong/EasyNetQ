using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace EasyNetQ.AMQP
{
    /// <summary>
    /// A connection that attempts to reconnect if the inner connection is closed.
    /// </summary>
    public class PersistentConnection : IPersistentConnection
    {
        private readonly IConnectionFactory connectionFactory;
        private readonly IEasyNetQLogger logger;
        private IConnection connection;
        private readonly IConnectionRetryTimer connectionRetryTimer;
        private readonly IChannelFactory channelFactory;

        public PersistentConnection(
            IConnectionFactory connectionFactory, 
            IEasyNetQLogger logger, 
            IConnectionRetryTimer connectionRetryTimer, 
            IChannelFactory channelFactory)
        {
            this.connectionFactory = connectionFactory;
            this.logger = logger;
            this.connectionRetryTimer = connectionRetryTimer;
            this.channelFactory = channelFactory;
        }

        public event Action Connected;
        public event Action Disconnected;

        public IModel CreateModel()
        {
            if(!IsConnected)
            {
                throw new EasyNetQException("Rabbit server is not connected.");
            }
            return connection.CreateModel();
        }

        public void Close()
        {
            connection.Close();
        }

        public IChannel OpenChannel()
        {
            return OpenChannel(new ChannelSettings());
        }

        public IChannel OpenChannel(IChannelSettings settings)
        {
            if (!IsConnected)
            {
                throw new EasyNetQException("Rabbit server is not connected.");
            }
            return channelFactory.OpenChannel(connection, settings);
        }

        public bool IsConnected
        {
            get { return connection != null && connection.IsOpen && !disposed; }
        }

        void StartTryToConnect()
        {
            connectionRetryTimer.ReTry(TryToConnect);
        }

        public void TryToConnect()
        {
            logger.DebugWrite("Trying to connect");
            if (disposed)
            {
                logger.ErrorWrite("Cannot connect a disposed connection");
                return;
            }

            connectionFactory.Reset();
            do
            {
                try
                {
                    connection = connectionFactory.CreateConnection();
                    connectionFactory.Success();
                }
                catch (System.Net.Sockets.SocketException socketException)
                {
                    LogException(socketException);
                }
                catch (BrokerUnreachableException brokerUnreachableException)
                {
                    LogException(brokerUnreachableException);
                }
            } while (connectionFactory.Next());

            if (connectionFactory.Succeeded)
            {
                connection.ConnectionShutdown += OnConnectionShutdown;

                OnConnected();
                logger.InfoWrite("Connected to RabbitMQ. Broker: '{0}', Port: {1}, VHost: '{2}'",
                    connectionFactory.CurrentHost.Host,
                    connectionFactory.CurrentHost.Port,
                    connectionFactory.Configuration.VirtualHost);
            }
            else
            {
                logger.ErrorWrite("Failed to connected to any Broker. Retrying in {0} seconds", 
                    connectionRetryTimer.RetryIntervalSeconds);
                StartTryToConnect();
            }
        }

        void LogException(Exception exception)
        {
            logger.ErrorWrite("Failed to connect to Broker: '{0}', Port: {1} VHost: '{2}'. " +
                    "ExceptionMessage: '{3}'",
                connectionFactory.CurrentHost.Host,
                connectionFactory.CurrentHost.Port,
                connectionFactory.Configuration.VirtualHost,
                exception.Message);
        }

        void OnConnectionShutdown(IConnection _, ShutdownEventArgs reason)
        {
            if (disposed) return;
            OnDisconnected();

            // try to reconnect and re-subscribe
            logger.InfoWrite("Disconnected from RabbitMQ Broker. Connection reset by {0}. Reason: '{1}'",
                reason.Initiator, reason.ReplyText);

            TryToConnect();
        }

        public void OnConnected()
        {
            logger.DebugWrite("OnConnected event fired");
            if (Connected != null) Connected();
        }

        public void OnDisconnected()
        {
            if (Disconnected != null) Disconnected();
        }

        private bool disposed = false;
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            if (connection != null) connection.Dispose();
        }
    }
}