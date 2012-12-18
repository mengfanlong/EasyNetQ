using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace EasyNetQ.AMQP
{
    public class Channel : IChannel
    {
        private readonly IModel model;

        public Channel(IModel model)
        {
            if(model == null)
            {
                throw new ArgumentNullException("model");
            }

            this.model = model;
        }

        public void Dispose()
        {
            model.Dispose();
        }

        public void Declare(IExchange exchange)
        {
            if(exchange == null)
            {
                throw new ArgumentNullException("exchange");
            }

            model.ExchangeDeclare(
                exchange.Name, 
                exchange.Type, 
                exchange.Durable, 
                exchange.AutoDelete, 
                exchange.Arguments.ToLegacyDictionary());
        }

        public bool Exists(IExchange exchange)
        {
            if(exchange == null)
            {
                throw new ArgumentNullException("exchange");
            }

            try
            {
                model.ExchangeDeclarePassive(exchange.Name);
                return true;
            }
            catch (OperationInterruptedException operationInterruptedException)
            {
                if (operationInterruptedException.ShutdownReason.ReplyCode == 404)
                {
                    return false;
                }
                throw;
            }
        }

        public void Delete(IExchange exchange)
        {
            if(exchange == null)
            {
                throw new ArgumentNullException("exchange");
            }

            model.ExchangeDelete(exchange.Name);
        }

        public QueueDeclareResult Declare(IQueue queue)
        {
            if(queue == null)
            {
                throw new ArgumentNullException("queue");
            }

            var queueDeclareOk = model.QueueDeclare(
                queue.Name,
                queue.Durable,
                queue.Exclusive,
                queue.AutoDelete,
                queue.Arguments.ToLegacyDictionary());

            return new QueueDeclareResult(
                queueDeclareOk.ConsumerCount,
                queueDeclareOk.MessageCount,
                queueDeclareOk.QueueName,
                true);
        }

        public IQueue DeclareBrokerGeneratedQueue()
        {
            var queueDeclareOk = model.QueueDeclare();
            var settings = new QueueSettings
            {
                Durable = false,
                Exclusive = true,
                AutoDelete = true
            };

            return Queue.Create(queueDeclareOk.QueueName, settings);
        }

        public QueueDeclareResult Exists(IQueue queue)
        {
            if(queue == null)
            {
                throw new ArgumentNullException("queue");
            }

            try
            {
                var queueDeclareOk = model.QueueDeclarePassive(queue.Name);
                return new QueueDeclareResult(
                    queueDeclareOk.ConsumerCount,
                    queueDeclareOk.MessageCount,
                    queueDeclareOk.QueueName,
                    true);
            }
            catch (OperationInterruptedException operationInterruptedException)
            {
                if (operationInterruptedException.ShutdownReason.ReplyCode == 404)
                {
                    return new QueueDeclareResult(0, 0, queue.Name, false);
                }
                throw;
            }
        }

        public QueuePurgeResult Delete(IQueue queue)
        {
            if(queue == null)
            {
                throw new ArgumentNullException("queue");
            }

            var numberOfMessagesPurged = model.QueueDelete(queue.Name);
            return new QueuePurgeResult(numberOfMessagesPurged);
        }

        public QueuePurgeResult Delete(IQueue queue, QueueDeletionCriteria criteria)
        {
            if(queue == null)
            {
                throw new ArgumentNullException("queue");
            }
            if(criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            var numberOfMessagesPurged = model.QueueDelete(queue.Name, 
                criteria.DeleteIfUnused, criteria.DeleteIfEmpty);

            return new QueuePurgeResult(numberOfMessagesPurged);
        }

        public QueuePurgeResult Purge(IQueue queue)
        {
            if(queue == null)
            {
                throw new ArgumentNullException("queue");
            }

            var numberOfMessagesPurged = model.QueuePurge(queue.Name);
            return new QueuePurgeResult(numberOfMessagesPurged);
        }

        public void Bind(IExchange source, IExchange destination, string routingKey)
        {
            if(source == null)
            {
                throw new ArgumentNullException("source");
            }
            if(destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            if(routingKey == null)
            {
                throw new ArgumentNullException("routingKey");
            }

            model.ExchangeBind(destination.Name, source.Name, routingKey);
        }

        public void Bind(IExchange exchange, IQueue queue, string routingKey)
        {
            Bind(exchange, queue, routingKey, new Arguments());
        }

        public void Bind(IExchange exchange, IQueue queue, string routingKey, Arguments arguments)
        {
            if(exchange == null)
            {
                throw new ArgumentNullException("exchange");
            }
            if(queue == null)
            {
                throw new ArgumentNullException("queue");
            }
            if(routingKey == null)
            {
                throw new ArgumentNullException("routingKey");
            }
            if(arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }

            model.QueueBind(queue.Name, exchange.Name, routingKey, arguments.ToLegacyDictionary());
        }

        public void UnBind(IExchange source, IExchange destination, string routingKey)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            if (routingKey == null)
            {
                throw new ArgumentNullException("routingKey");
            }

            model.ExchangeUnbind(destination.Name, source.Name, routingKey);
        }

        public void UnBind(IExchange exchange, IQueue queue, string routingKey, Arguments arguments)
        {
            if (exchange == null)
            {
                throw new ArgumentNullException("exchange");
            }
            if (queue == null)
            {
                throw new ArgumentNullException("queue");
            }
            if (routingKey == null)
            {
                throw new ArgumentNullException("routingKey");
            }
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }

            model.QueueUnbind(queue.Name, exchange.Name, routingKey, arguments.ToLegacyDictionary());
        }
    }
}