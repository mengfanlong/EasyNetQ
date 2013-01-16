// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using EasyNetQ.AMQP;
using EasyNetQ.Loggers;
using NUnit.Framework;

namespace EasyNetQ.Tests.AMQP
{
    [TestFixture]
    [Explicit("Required a RabbitMQ server on localhost to work")]
    public class AmqpIntegrationTests
    {
        private IPersistentConnection connection;
        private const string queueName = "intergration_test_queue";
        private IQueue queue;

        [SetUp]
        public void SetUp()
        {
            var configuration = new ConnectionConfiguration
            {
                Hosts = new List<IHostConfiguration>
                {
                    new HostConfiguration { Host = "localhost" }
                }
            };
            configuration.Validate();

            IConnectionFactory connectionFactory = new ConnectionFactoryWrapper(
                configuration, 
                new DefaultClusterHostSelectionStrategy<ConnectionFactoryInfo>()
                );

            connection = new PersistentConnection(
                connectionFactory, 
                new ConsoleLogger(), 
                new ConnectionRetryTimer(configuration), 
                new ChannelFactory());

            connection.TryToConnect();

            queue = Queue.Create(queueName);
        }

        [TearDown]
        public void TearDown()
        {
            connection.Dispose();
        }

        [Test]
        public void Should_be_able_to_publish()
        {
            using(var channel = connection.OpenChannel())
            {
                channel.Declare(queue);

                var exchange = Exchange.Direct("some_exchange");
                channel.Declare(exchange);
                channel.Bind(exchange, queue, queueName);

                var settings = new PublishSettings(exchange, queueName);
                var message = new RawMessage(Encoding.UTF8.GetBytes("Hello World!"));

                channel.Publish(message, settings);
            }
        }

        [Test]
        [Explicit("Run this test after placing a message on the queue by running the test above")]
        public void Should_be_able_to_consume()
        {
            var autoResetEvent = new AutoResetEvent(false);

            using (var channel = connection.OpenChannel())
            {
                channel.Declare(queue);

                var settings = new ConsumerSettings(queue)
                {
                    ConsumerTag = Guid.NewGuid().ToString()
                };

                var consumer = CreateConsumer(autoResetEvent);

                channel.StartConsuming(consumer, settings);

                autoResetEvent.WaitOne(TimeSpan.FromSeconds(10));
            }
        }

        [Test]
        [Explicit("Run this test after placing a message on the queue by running the test above")]
        public void Should_be_able_to_maintain_a_persisten_consumer()
        {
            var autoResetEvent = new AutoResetEvent(false);
            var persistentConsumer = new PersistentConsumer(connection);

            var consumer = CreateConsumer(autoResetEvent);
            var settings = new ConsumerSettings(queue)
            {
                ConsumerTag = Guid.NewGuid().ToString()
            };

            // first cause a connection bounce
            ((PersistentConnection)connection).Close();

            persistentConsumer.StartConsuming(consumer, settings, new ChannelSettings());
            Console.Out.WriteLine("Conusmer started");

            autoResetEvent.WaitOne(TimeSpan.FromSeconds(10));
//            Thread.Sleep(TimeSpan.FromSeconds(60));
        }

        private static Consumer CreateConsumer(EventWaitHandle waitHandle)
        {
            var loop = new QueueingConsumerLoop();
            var handlerSelector = new BasicHandlerSelector();
            handlerSelector.SetHandler(message =>
            {
                var stringMessage = Encoding.UTF8.GetString(message.Body);
                Console.Out.WriteLine("Got Message: '{0}'", stringMessage);
                waitHandle.Set();
            });
            var executionPolicyBuilder = new DefaultExecutionPolicyBuilder();

            var consumer = new Consumer(
                loop,
                handlerSelector,
                executionPolicyBuilder
                );
            return consumer;
        }
    }
}

// ReSharper restore InconsistentNaming