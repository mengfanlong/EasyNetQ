using System;

namespace EasyNetQ.AMQP
{
    public interface IConnectionRetryTimer
    {
        void ReTry(Action retryAction);
        ushort RetryIntervalSeconds { get; }
    }
}