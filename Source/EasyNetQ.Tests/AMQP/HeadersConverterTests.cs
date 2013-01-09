// ReSharper disable InconsistentNaming

using System.Collections;
using EasyNetQ.AMQP;
using NUnit.Framework;

namespace EasyNetQ.Tests.AMQP
{
    [TestFixture]
    public class HeadersConverterTests
    {
        private HeadersConverter headersConverter;

        [SetUp]
        public void SetUp()
        {
            headersConverter = new HeadersConverter();
        }

        [Test]
        public void Should_convert_Headers_to_dictionary()
        {
            var headers = new Headers
            {
                { "Key1", "Value1" },
                { "Key2", "Value2" }
            };

            var dictionary = (IDictionary) headersConverter.ConvertFromEasyNetQValue(headers);

            dictionary["Key1"].ShouldEqual("Value1");
            dictionary["Key2"].ShouldEqual("Value2");

        }
    }
}

// ReSharper restore InconsistentNaming