// ReSharper disable InconsistentNaming

using System.Collections;
using EasyNetQ.AMQP;
using NUnit.Framework;
using RabbitMQ.Client;
using Rhino.Mocks;
using Queue = EasyNetQ.AMQP.Queue;

namespace EasyNetQ.Tests.AMQP
{
    [TestFixture]
    public class ConsumeTests
    {
        private IModel model;
        private IChannel channel;

        [SetUp]
        public void SetUp()
        {
            model = MockRepository.GenerateStub<IModel>();
            channel = new Channel(model);

            model.Stub(x => x.CreateBasicProperties()).Return(new TestBasicProperties());
        }

        [Test]
        public void Should_be_able_to_start_consuming()
        {
            var queue = Queue.Create("my_queue");
            var settings = new ConsumerSettings(queue)
            {
                ConsumerTag = "consumer_tag",
                NoAck = true,
                Exclusive = true
            };
            settings.Arguments.Add("key1", "value1");
            settings.Arguments.Add("key2", "value2");

            var consumer = MockRepository.GenerateStub<IConsumer>();

            channel.StartConsuming(consumer, settings);

            model.AssertWasCalled(x => x.BasicConsume(
                Arg<string>.Is.Equal("my_queue"),
                Arg<bool>.Is.Equal(true),               // NoAck
                Arg<string>.Is.Equal("consumer_tag"),
                Arg<bool>.Is.Equal(false),              // NoLocal
                Arg<bool>.Is.Equal(true),               // Exclusive
                Arg<IDictionary>.Matches(args => 
                    (string)args["key1"] == "value1" && 
                    (string)args["key2"] == "value2"),
                Arg<IBasicConsumer>.Matches(c => c is EasyNetQBasicConsumer)));
        }

        [Test]
        public void Should_be_able_to_ack()
        {
            const ulong deliveryTag = 222;
            const bool multiple = false;

            channel.Acknowledge(deliveryTag, multiple);

            model.AssertWasCalled(x => x.BasicAck(deliveryTag, multiple));
        }

        [Test]
        public void Should_be_able_to_nack()
        {
            const ulong deliveryTag = 222;
            const bool requeue = false;

            channel.Reject(deliveryTag, requeue);

            model.AssertWasCalled(x => x.BasicNack(deliveryTag, false, requeue));
        }
    }
}

// ReSharper restore InconsistentNaming