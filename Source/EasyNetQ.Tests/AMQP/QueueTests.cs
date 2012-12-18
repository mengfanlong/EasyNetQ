// ReSharper disable InconsistentNaming

using EasyNetQ.AMQP;
using NUnit.Framework;

namespace EasyNetQ.Tests.AMQP
{
    [TestFixture]
    public class QueueTests
    {
        private QueueSettings settings;

        [SetUp]
        public void SetUp()
        {
            settings = new QueueSettings();
        }

        [Test]
        public void Should_have_correct_default_settings_values()
        {
            settings.AutoDelete.ShouldBeFalse();
            settings.Durable.ShouldBeTrue();
            settings.Exclusive.ShouldBeFalse();
        }

        [Test]
        public void Should_be_able_to_create_a_queue()
        {
            settings.AutoDelete = true;
            settings.Durable = true;
            settings.Exclusive = true;

            var queue = Queue.Create("my_queue", settings)
                .AddArgument("key1", "value1")
                .AddArgument("key2", "value2");

            queue.Name.ShouldEqual("my_queue");
            queue.AutoDelete.ShouldBeTrue();
            queue.Durable.ShouldBeTrue();
            queue.Exclusive.ShouldBeTrue();
            queue.Arguments["key1"].ShouldEqual("value1");
            queue.Arguments["key2"].ShouldEqual("value2");
        }
    }
}

// ReSharper restore InconsistentNaming