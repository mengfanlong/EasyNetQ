using System;
using System.Collections.Concurrent;
using System.Threading;
using EasyNetQ.Patterns;

namespace EasyNetQ.AMQP
{
    /// <summary>
    /// Creates an internal queue which marshalls publish calls onto a single thread
    /// for publication on a single channel.
    /// </summary>
    public class PublishDispatcher : IPublishDispatcher
    {
        private readonly IPersistentChannel persistentChannel;
        private readonly IEasyNetQLogger logger;
        private readonly IExchangeManager exchangeManager;

        private readonly AutoResetEvent channelErrorWait = new AutoResetEvent(false);

        private bool isInitialised;
        private readonly BlockingCollection<PublishContext> publishQueue = 
            new BlockingCollection<PublishContext>();

        private Thread publishThread;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public PublishDispatcher(
            IPersistentChannel persistentChannel, 
            IEasyNetQLogger logger, 
            IExchangeManager exchangeManager)
        {
            this.persistentChannel = persistentChannel;
            this.logger = logger;
            this.exchangeManager = exchangeManager;
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
                        if (exchangeManager.ShouldDeclare(publishContext.PublishSettings.Exchange))
                        {
                            persistentChannel.Declare(publishContext.PublishSettings.Exchange);
                            exchangeManager.Declared(publishContext.PublishSettings.Exchange);
                        }

                        persistentChannel.Publish(publishContext.RawMessage, publishContext.PublishSettings);
                    }
                    catch (RabbitMQ.Client.Exceptions.OperationInterruptedException operationInterruptedException)
                    {
                        logger.InfoWrite("Exception on publish.{0}", 
                            operationInterruptedException.ToString());

                        // wait a little while before looping and retrying.
                        // but continue if signalled.
                        channelErrorWait.WaitOne(TimeSpan.FromSeconds(1));

                        // if we've been disposed just drop out of loop
                        if (cancellationTokenSource.IsCancellationRequested) return;

                        // if the publish fails we should probably leave the publish message on the queue
                        logger.InfoWrite("Requeueing message");
                        publishQueue.Add(publishContext);
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
            channelErrorWait.Set();
            cancellationTokenSource.Cancel();
        }
    }
}