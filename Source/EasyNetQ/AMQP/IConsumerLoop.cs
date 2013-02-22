using System;
using System.Collections.Concurrent;
using System.Threading;

namespace EasyNetQ.AMQP
{
    public interface IConsumerLoop : IDisposable
    {
        void QueueMessageHandleAction(Action action);
        void Start(string consumerTag);
        void Stop(string consumerTag);
    }

    public class QueueingConsumerLoop : IConsumerLoop
    {
        private readonly BlockingCollection<Action> messageQueue = new BlockingCollection<Action>();

        private readonly Thread consumerThread;
        private volatile bool running = true;

        public QueueingConsumerLoop()
        {
            consumerThread = new Thread(() =>
            {
                while (running)
                {
                    var action = messageQueue.Take();
                    action();
                }
            }){ Name = "EasyNetQ consumer loop" };

            consumerThread.Start();
        }

        public void QueueMessageHandleAction(Action action)
        {
            if (!running)
            {
                throw new EasyNetQAmqpException("Consumer loop is no longer running.");
            }

            messageQueue.Add(action);
        }

        public void Start(string consumerTag)
        {
            throw new NotImplementedException();
        }

        public void Stop(string consumerTag)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            messageQueue.CompleteAdding();
            running = false;
        }
    }
}