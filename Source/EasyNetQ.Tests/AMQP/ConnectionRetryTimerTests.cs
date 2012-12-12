// ReSharper disable InconsistentNaming

using System;
using System.Threading;
using EasyNetQ.AMQP;
using NUnit.Framework;

namespace EasyNetQ.Tests.AMQP
{
    [TestFixture]
    public class ConnectionRetryTimerTests
    {
        private IConnectionRetryTimer connectionRetryTimer;

        [SetUp]
        public void SetUp()
        {
            connectionRetryTimer = new ConnectionRetryTimer(new ConnectionConfiguration
            {
                ConnectionRetrySeconds = 1
            });
        }

        [Test]
        public void Should_retry_given_action_after_one_second()
        {
            var autoResetEvent = new AutoResetEvent(false);
            var retryOccured = false;

            connectionRetryTimer.ReTry(() =>
            {
                retryOccured = true;
                autoResetEvent.Set();
            });

            autoResetEvent.WaitOne(TimeSpan.FromSeconds(2));

            retryOccured.ShouldBeTrue();
        }
    }
}

// ReSharper restore InconsistentNaming