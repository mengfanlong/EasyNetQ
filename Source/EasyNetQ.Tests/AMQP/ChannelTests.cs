// ReSharper disable InconsistentNaming

using System;
using System.Collections;
using EasyNetQ.AMQP;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Rhino.Mocks;
using Queue = EasyNetQ.AMQP.Queue;

namespace EasyNetQ.Tests.AMQP
{
    [TestFixture]
    public class ChannelTests
    {
        private IModel model;
        private IChannel channel;

        [SetUp]
        public void SetUp()
        {
            model = MockRepository.GenerateStub<IModel>();
            channel = new Channel(model);
        }

        [Test]
        public void Should_dispose_model_when_disposed()
        {
            channel.Dispose();
            model.AssertWasCalled(x => x.Dispose());
        }

        [Test]
        public void Should_be_able_to_declare_an_exchange()
        {
            var exchange = Exchange.Direct("my_exchange")
                .AddArgument("key1", "value1")
                .AddArgument("key2", "value2");

            channel.Declare(exchange);

            model.AssertWasCalled(x => x.ExchangeDeclare(
                Arg<string>.Is.Equal("my_exchange"),
                Arg<string>.Is.Equal("direct"),
                Arg<bool>.Is.Equal(true),
                Arg<bool>.Is.Equal(false),
                Arg<IDictionary>.Matches(dictionary => 
                    (string)dictionary["key1"] == "value1" && 
                    (string)dictionary["key2"] == "value2")));
        }

        [Test]
        public void Should_be_able_to_passively_declare_an_exchange()
        {
            var exchange = Exchange.Direct("my_exchange");

            var result = channel.Exists(exchange);

            result.ShouldBeTrue();
            model.AssertWasCalled(x => x.ExchangeDeclarePassive("my_exchange"));
        }

        [Test]
        public void Should_return_false_if_passively_declare_an_exchange_which_doesnt_exit()
        {
            var exceptionArgs = new ShutdownEventArgs(ShutdownInitiator.Peer, 404, "some text");
            var exception = new OperationInterruptedException(exceptionArgs);
            model.Stub(x => x.ExchangeDeclarePassive("my_exchange")).Throw(exception);
            var exchange = Exchange.Direct("my_exchange");

            var result = channel.Exists(exchange);

            result.ShouldBeFalse();
        }

        [Test]
        public void Should_be_able_to_delete_an_exchange()
        {
            var exchange = Exchange.Direct("my_exchange");

            channel.Delete(exchange);

            model.AssertWasCalled(x => x.ExchangeDelete("my_exchange"));
        }

        [Test]
        public void Should_be_able_to_declare_a_queue()
        {
            model.Stub(x => x.QueueDeclare(
                Arg<string>.Is.Anything,
                Arg<bool>.Is.Anything,
                Arg<bool>.Is.Anything,
                Arg<bool>.Is.Anything,
                Arg<IDictionary>.Is.Anything
                )).Return(new QueueDeclareOk("my_queue", 101, 9));

            var settings = new QueueSettings
            {
                AutoDelete = true,
                Durable = true,
                Exclusive = true
            };

            var queue = Queue.Create("my_queue", settings)
                .AddArgument("key1", "value1")
                .AddArgument("key2", "value2");

            var result = channel.Declare(queue);

            model.AssertWasCalled(x => x.QueueDeclare(
                Arg<string>.Is.Equal("my_queue"),
                Arg<bool>.Is.Equal(true),
                Arg<bool>.Is.Equal(true),
                Arg<bool>.Is.Equal(true),
                Arg<IDictionary>.Matches(dictionary =>
                    (string)dictionary["key1"] == "value1" &&
                    (string)dictionary["key2"] == "value2")));

            result.ConsumerCount.ShouldEqual(9);
            result.MessageCount.ShouldEqual(101);
            result.QueueName.ShouldEqual("my_queue");
            result.QueueExists.ShouldBeTrue();
        }

        [Test]
        public void Should_be_able_to_declare_a_broker_generated_queue()
        {
            model.Stub(x => x.QueueDeclare()).Return(new QueueDeclareOk("abc_xyz", 0, 0));

            var queue = channel.DeclareBrokerGeneratedQueue();

            queue.Name.ShouldEqual("abc_xyz");
            queue.Durable.ShouldBeFalse();
            queue.Exclusive.ShouldBeTrue();
            queue.AutoDelete.ShouldBeTrue();
            queue.Arguments.Count.ShouldEqual(0);
        }

        [Test]
        public void Should_be_able_to_passively_declare_a_queue()
        {
            model.Stub(x => x.QueueDeclarePassive("my_queue")).Return(new QueueDeclareOk("my_queue", 101, 9));
            var queue = Queue.Create("my_queue");

            var result = channel.Exists(queue);

            result.ConsumerCount.ShouldEqual(9);
            result.MessageCount.ShouldEqual(101);
            result.QueueName.ShouldEqual("my_queue");
            result.QueueExists.ShouldBeTrue();
        }

        [Test]
        public void Should_show_queue_doesnt_exist_on_declare_passive()
        {
            var exceptionArgs = new ShutdownEventArgs(ShutdownInitiator.Peer, 404, "some text");
            var exception = new OperationInterruptedException(exceptionArgs);
            model.Stub(x => x.QueueDeclarePassive("my_queue")).Throw(exception);
            var queue = Queue.Create("my_queue");

            var result = channel.Exists(queue);

            result.QueueExists.ShouldBeFalse();
        }

        [Test]
        public void Should_be_able_to_delete_a_queue_unconditionally()
        {
            model.Stub(x => x.QueueDelete("my_queue")).Return(345);
            var queue = Queue.Create("my_queue");

            var result = channel.Delete(queue);

            result.NumberOfMessagesPurged.ShouldEqual(345);
        }

        [Test]
        public void Should_be_able_to_delete_a_queue_conditionally()
        {
            model.Stub(x => x.QueueDelete("my_queue", true, true)).Return(345);
            var criteria = new QueueDeletionCriteria(true, true);
            var queue = Queue.Create("my_queue");

            var result = channel.Delete(queue, criteria);

            result.NumberOfMessagesPurged.ShouldEqual(345);
        }

        [Test]
        public void Should_be_able_to_purge_a_queue()
        {
            model.Stub(x => x.QueuePurge("my_queue")).Return(345);
            var queue = Queue.Create("my_queue");

            var result = channel.Purge(queue);

            result.NumberOfMessagesPurged.ShouldEqual(345);
        }

        [Test]
        public void Should_be_able_to_bind_an_exchange_to_an_exchange()
        {
            var source = Exchange.Direct("my_source_exchange");
            var destination = Exchange.Direct("my_destination_exchange");
            const string routingKey = "the_routing_key";

            channel.Bind(source, destination, routingKey);

            model.AssertWasCalled(x => x.ExchangeBind("my_destination_exchange", "my_source_exchange", routingKey));
        }

        [Test]
        public void Should_be_able_to_unbind_an_exchange_to_exchange_binding()
        {
            var source = Exchange.Direct("my_source_exchange");
            var destination = Exchange.Direct("my_destination_exchange");
            const string routingKey = "the_routing_key";

            channel.UnBind(source, destination, routingKey);

            model.AssertWasCalled(x => x.ExchangeUnbind("my_destination_exchange", "my_source_exchange", routingKey));
        }

        [Test]
        public void Should_be_able_to_bind_an_exchange_to_a_queue()
        {
            var exchange = Exchange.Direct("my_exchange");
            var queue = Queue.Create("my_queue");
            const string routingKey = "the_routing_key";
            var arguments = new Arguments
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };

            channel.Bind(exchange, queue, routingKey, arguments);

            model.AssertWasCalled(x => x.QueueBind(
                Arg<string>.Is.Equal("my_queue"),
                Arg<string>.Is.Equal("my_exchange"),
                Arg<string>.Is.Equal("the_routing_key"),
                Arg<IDictionary>.Matches(dictionary => 
                    (string)dictionary["key1"] == "value1" && 
                    (string)dictionary["key2"] == "value2" )));
        }

        [Test]
        public void Should_be_able_to_unbind_an_exchange_to_queue_binding()
        {
            var exchange = Exchange.Direct("my_exchange");
            var queue = Queue.Create("my_queue");
            const string routingKey = "the_routing_key";
            var arguments = new Arguments
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };

            channel.UnBind(exchange, queue, routingKey, arguments);

            model.AssertWasCalled(x => x.QueueUnbind(
                Arg<string>.Is.Equal("my_queue"),
                Arg<string>.Is.Equal("my_exchange"),
                Arg<string>.Is.Equal("the_routing_key"),
                Arg<IDictionary>.Matches(dictionary =>
                    (string)dictionary["key1"] == "value1" &&
                    (string)dictionary["key2"] == "value2")));
        }

        [Test]
        public void Should_fire_ChannlClosedEvent_when_model_shutdown_fires()
        {
            var closedFired = false;
            channel.ChannelClosed += () => closedFired = true;

            model.Raise(x => x.ModelShutdown += null, model, new ShutdownEventArgs(ShutdownInitiator.Peer, 0, ""));

            closedFired.ShouldBeTrue();
        }
    }
}

// ReSharper restore InconsistentNaming