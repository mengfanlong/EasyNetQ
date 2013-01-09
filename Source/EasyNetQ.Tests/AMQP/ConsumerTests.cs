// ReSharper disable InconsistentNaming

using System;
using EasyNetQ.AMQP;
using NUnit.Framework;
using Rhino.Mocks;

namespace EasyNetQ.Tests.AMQP
{
    [TestFixture]
    public class ConsumerTests
    {
        private IConsumer consumer;
        private IConsumerLoop consumerLoop;
        private IHandlerSelector handlerSelector;
        private IExecutionPolicyBuilder executionPolicyBuilder;

        [SetUp]
        public void SetUp()
        {
            consumerLoop = MockRepository.GenerateStub<IConsumerLoop>();
            handlerSelector = MockRepository.GenerateStub<IHandlerSelector>();
            executionPolicyBuilder = MockRepository.GenerateStub<IExecutionPolicyBuilder>();

            consumer = new Consumer(
                consumerLoop, 
                handlerSelector, 
                executionPolicyBuilder);
        }

        [Test]
        public void Should_handle_message_delivery()
        {
            var messageDeliveryContext = MockRepository.GenerateStub<IMessageDeliveryContext>();

            executionPolicyBuilder
                .Stub(x => x.BuildExecutionPolicy(Arg<Func<IMessageDeliveryContext, IHandler>>.Is.Anything))
                .Return(x => {});

            consumer.HandleMessage(messageDeliveryContext);

            consumerLoop.AssertWasCalled(x => x.QueueMessageHandleAction(Arg<Action>.Is.Anything));
        }
    }
}

// ReSharper restore InconsistentNaming