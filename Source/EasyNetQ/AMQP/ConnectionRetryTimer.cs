using System;
using System.Threading;

namespace EasyNetQ.AMQP
{
    public class ConnectionRetryTimer : IConnectionRetryTimer
    {
        private readonly IConnectionConfiguration connectionConfiguration;

        public ConnectionRetryTimer(IConnectionConfiguration connectionConfiguration)
        {
            this.connectionConfiguration = connectionConfiguration;
        }

        public void ReTry(Action retryAction)
        {
            var timer = new Timer(CreateTimerCallback(retryAction));
            timer.Change(connectionConfiguration.ConnectionRetrySeconds * 1000, Timeout.Infinite);
        }

        public ushort RetryIntervalSeconds
        {
            get { return connectionConfiguration.ConnectionRetrySeconds; }
        }

        public TimerCallback CreateTimerCallback(Action retryAction)
        {
            return timer =>
            {
                ((Timer)timer).Dispose();
                retryAction();
            };
        }
    }
}