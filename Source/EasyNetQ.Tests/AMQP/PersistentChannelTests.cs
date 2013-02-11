// ReSharper disable InconsistentNaming

using System;
using EasyNetQ.AMQP;
using NUnit.Framework;
using Rhino.Mocks;

namespace EasyNetQ.Tests.AMQP
{
    [TestFixture]
    public class PersistentChannelTests
    {
        private IPersistentChannel persistentChannel;
        private IPersistentConnection persistentConnection;
        private IChannel channel;
        private IChannelSettings channelSettings;
        private int createChannelCount;
        private int channelOpenedFiredCount;
        private int channelClosedFiredCount;

        [SetUp]
        public void SetUp()
        {
            createChannelCount = 0;
            channelOpenedFiredCount = 0;
            channelClosedFiredCount = 0;

            persistentConnection = MockRepository.GenerateStub<IPersistentConnection>();
            channel = MockRepository.GenerateStub<IChannel>();
            channelSettings = new ChannelSettings();

            persistentChannel = new PersistentChannel();

            persistentChannel.ChannelOpened += () => channelOpenedFiredCount++;
            persistentChannel.ChannelClosed += () => channelClosedFiredCount++;

            persistentConnection.Stub(x => x.OpenChannel(channelSettings))
                .Callback<IChannelSettings>(x =>
                {
                    createChannelCount++;
                    return true;
                })
                .Return(channel);
            persistentChannel.Initialise(persistentConnection, channelSettings);
        }

        [Test]
        public void Should_create_internal_channel()
        {
            createChannelCount.ShouldEqual(1);
            
            channelOpenedFiredCount.ShouldEqual(1);
            channelClosedFiredCount.ShouldEqual(0);
        }

        [Test]
        public void Should_recreate_channel_after_disconnect()
        {
            channel.Raise(x => x.ChannelClosed += null, new object[0]);
            createChannelCount.ShouldEqual(2);

            channelOpenedFiredCount.ShouldEqual(2);
            channelClosedFiredCount.ShouldEqual(1);
        }

        [Test]
        public void Should_wait_for_reconnection_of_connection_when_channel_disconnects()
        {
            persistentConnection.ClearBehavior();

            // overly complex stub:
            // first time it's called raise an exception, second time just increment channel count
            var firstTime = true;
            persistentConnection.Stub(x => x.OpenChannel(channelSettings))
                .Callback<IChannelSettings>(x =>
                {
                    if (firstTime)
                    {
                        firstTime = false;
                        throw new Exception("Exception from OpenChannel");
                    }

                    createChannelCount++;
                    return true;
                })
                .Return(channel);

            channel.Raise(x => x.ChannelClosed += null, new object[0]);

            // shouldn't have reconnected.
            createChannelCount.ShouldEqual(1);

            // raise connection connected event
            persistentConnection.Raise(x => x.Connected += null, new object[0]);

            // should have reconnected.
            createChannelCount.ShouldEqual(2);

            channelOpenedFiredCount.ShouldEqual(2);
            channelClosedFiredCount.ShouldEqual(1);
        }
    }
}

// ReSharper restore InconsistentNaming