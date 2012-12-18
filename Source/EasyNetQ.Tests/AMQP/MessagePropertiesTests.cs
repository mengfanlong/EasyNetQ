// ReSharper disable InconsistentNaming

using NUnit.Framework;

namespace EasyNetQ.Tests.AMQP
{
    [TestFixture]
    public class MessagePropertiesTests
    {
        private EasyNetQ.AMQP.MessageProperties messageProperties;

        [SetUp]
        public void SetUp()
        {
            messageProperties = new EasyNetQ.AMQP.MessageProperties();
        }

        [Test]
        public void Should_not_have_null_properties_on_creation()
        {
            messageProperties.AppId.ShouldNotBeNull();
        }

        [Test]
        public void Should_not_be_set_on_creation()
        {
            messageProperties.AppId.IsSet.ShouldBeFalse();
        }

        [Test]
        public void Should_be_set_after_property_has_been_set()
        {
            messageProperties.AppId.Value = "my app id";
            messageProperties.AppId.IsSet.ShouldBeTrue();
        }

        [Test]
        public void Should_have_the_set_value()
        {
            messageProperties.AppId.Value = "my app id";
            messageProperties.AppId.Value.ShouldEqual("my app id");
        }

        [Test, ExpectedException(typeof(EasyNetQ.AMQP.PropertyNotSetException))]
        public void Should_throw_if_unset_value_is_accessed()
        {
            var somevalue = messageProperties.AppId.Value;
        }

        [Test]
        public void Should_be_able_to_clear_a_property()
        {
            messageProperties.AppId.Value = "my app id";
            messageProperties.AppId.Clear();
            messageProperties.AppId.IsSet.ShouldBeFalse();
        }
    }
}

// ReSharper restore InconsistentNaming