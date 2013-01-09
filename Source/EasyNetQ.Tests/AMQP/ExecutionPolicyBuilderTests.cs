// ReSharper disable InconsistentNaming

using NUnit.Framework;
using EasyNetQ.AMQP;
using Rhino.Mocks;

namespace EasyNetQ.Tests.AMQP
{
    [TestFixture]
    public class ExecutionPolicyBuilderTests
    {
        private IExecutionPolicyBuilder executionPolicyBuilder;
        private IHandler handler;

        [SetUp]
        public void SetUp()
        {
            executionPolicyBuilder = new DefaultExecutionPolicyBuilder();
            handler = MockRepository.GenerateStub<IHandler>();
        }

        [Test]
        public void Should_create_an_execution_policy()
        {
            var messageDeliveryContext = MockRepository.GenerateStub<IMessageDeliveryContext>();

            var executionPolicy = executionPolicyBuilder.BuildExecutionPolicy(x => handler);

            executionPolicy(messageDeliveryContext);

            handler.AssertWasCalled(x => x.Handle(messageDeliveryContext));
        }
    }
}

// ReSharper restore InconsistentNaming