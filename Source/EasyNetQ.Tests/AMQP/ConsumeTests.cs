// ReSharper disable InconsistentNaming

using EasyNetQ.AMQP;
using NUnit.Framework;
using RabbitMQ.Client;
using Rhino.Mocks;

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
            var settings = new ConsumerSettings(queue);
            var consumerLoop = MockRepository.GenerateStub<IConsumerLoop>();

            var consumer = new Consumer(consumerLoop, settings);

            channel.StartConsuming(consumer);
        }
    }
}

// ReSharper restore InconsistentNaming