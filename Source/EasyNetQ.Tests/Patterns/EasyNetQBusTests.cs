// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Threading;
using EasyNetQ.AMQP;
using EasyNetQ.Loggers;
using EasyNetQ.Patterns;
using NUnit.Framework;

namespace EasyNetQ.Tests.Patterns
{
    [TestFixture]
    [Explicit("Integration test - requires a RabbitMQ broker on localhost to work")]
    public class EasyNetQBusTests
    {
        private EasyNetQBus bus;
        private IPersistentConnection persistentConnection;
        private IPersistentChannel persistentChannel;
        private IPublishDispatcher publishDispatcher;

        [SetUp]
        public void SetUp()
        {
            var logger = new ConsoleLogger();

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

            persistentConnection = new PersistentConnection(
                connectionFactory,
                logger,
                new ConnectionRetryTimer(configuration),
                new ChannelFactory(logger));

            persistentConnection.TryToConnect();

            IProducerPipelineBuilder producerPipelineBuilder = new ProducerPipelineBuilder();
            
            persistentChannel = new PersistentChannel();
            persistentChannel.Initialise(persistentConnection, new ChannelSettings());

            publishDispatcher = new PublishDispatcher(persistentChannel, logger, new ExchangeManager());
            publishDispatcher.Initialize(persistentConnection, new ChannelSettings());

            bus = new EasyNetQBus(
                producerPipelineBuilder, 
                persistentConnection, 
                persistentChannel, 
                publishDispatcher);
        }

        [TearDown]
        public void TearDown()
        {
            publishDispatcher.Dispose();
            persistentChannel.Dispose();
            persistentConnection.Dispose();
        }

        [Test]
        public void Should_publish_a_message()
        {
            var message = new MyMessage {Text = "Hello World!"};
            bus.Publish(message);

            Thread.Sleep(100);
        }

        [Test]
        public void Should_publish_a_load_of_messages_from_different_threads()
        {
            const int numberOfThreads = 10;

            for (var threadIndex = 0; threadIndex < numberOfThreads; threadIndex++)
            {
                var localTheadIndex = threadIndex;
                var thread = new Thread(() =>
                {
                    for (var messageIndex = 0; messageIndex < 1000; messageIndex++)
                    {
                        var message = new MyMessage
                        {
                            Text = string.Format("Message thread {0}, index {1}", localTheadIndex, messageIndex)
                        };

                        bus.Publish(message);
                        Console.Out.WriteLine("{0}", message.Text);
                    }
                });
                thread.Start();
            }

            // allow all the threads to start
            Thread.Sleep(TimeSpan.FromSeconds(10));
        }

        [Test]
        public void Should_subscribe_to_a_message()
        {
            bus.Subscribe<MyMessage>("my_queue", message => 
                Console.Out.WriteLine("message.Text = {0}", message.Text));

            // give the subscriber a chance to receive the message
            Thread.Sleep(500);
        }
    }
}

// ReSharper restore InconsistentNaming