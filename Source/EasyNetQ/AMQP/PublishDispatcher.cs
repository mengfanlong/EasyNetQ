using System;
using System.Collections.Concurrent;
using System.Threading;

namespace EasyNetQ.AMQP
{
    /// <summary>
    /// Creates an internal queue which marshalls publish calls onto a single thread
    /// for publication on a single channel.
    /// </summary>
    public class PublishDispatcher : IPublishDispatcher
    {
        private readonly IPersistentChannel persistentChannel;
        private bool isInitialised;
        private readonly BlockingCollection<PublishContext> publishQueue = 
            new BlockingCollection<PublishContext>();

        private Thread publishThread;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public PublishDispatcher(IPersistentChannel persistentChannel)
        {
            this.persistentChannel = persistentChannel;
        }

        public void Initialize(IPersistentConnection connection, IChannelSettings channelSettings)
        {
            if(channelSettings == null)
            {
                throw new ArgumentNullException("channelSettings");
            }

            if (cancellationTokenSource.IsCancellationRequested)
            {
                throw new EasyNetQPublishException("Cannot initialize PublishDispatcher, it is already disposed.");
            }

            persistentChannel.Initialise(connection, channelSettings);
            publishThread = new Thread(StartInternalPublishLoop);
            publishThread.Start();
            isInitialised = true;
        }

        public void Publish(IRawMessage message, IPublishSettings settings)
        {
            CheckIfPublishAllowed();

            if(message == null)
            {
                throw new ArgumentNullException("message");
            }
            if(settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            publishQueue.Add(new PublishContext(message, settings));
        }

        private void CheckIfPublishAllowed()
        {
            if (!isInitialised)
            {
                throw new EasyNetQPublishException("Publish is not allowed before the PublishDispatcher is initialized.");
            }

            if (cancellationTokenSource.IsCancellationRequested)
            {
                throw new EasyNetQPublishException("Publish is not allowed after the PublishDispatcher has been disposed.");
            }
        }

        private void StartInternalPublishLoop()
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var publishContext = publishQueue.Take(cancellationTokenSource.Token);
                    try
                    {
                        persistentChannel.Publish(publishContext.RawMessage, publishContext.PublishSettings);
                    }
                    catch (InvalidOperationException)
                    {
                        // if the publish fails we should probably leave the publish message on the queue
                        publishQueue.Add(publishContext);
                        // wait a little while before looping and retrying.
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }
                catch (OperationCanceledException)
                {
                    // no need to do anything here, this PublishDispatcher has been disposed
                    // and the publish loop will simply exit.
                }
            }
        }

        private class PublishContext
        {
            public IRawMessage RawMessage { get; private set; }
            public IPublishSettings PublishSettings { get; private set; }

            public PublishContext(IRawMessage rawMessage, IPublishSettings publishSettings)
            {
                RawMessage = rawMessage;
                PublishSettings = publishSettings;
            }
        }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
        }
    }
}