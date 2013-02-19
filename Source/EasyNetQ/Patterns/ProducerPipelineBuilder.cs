using System;
using EasyNetQ.AMQP;

namespace EasyNetQ.Patterns
{
    public interface IProducerPipelineBuilder
    {
        ProducerTransformer<int> CreateDefaultPublishPipeline<T>(T message);
    }

    public class ProducerPipelineBuilder : IProducerPipelineBuilder
    {
        public ProducerTransformer<int> CreateDefaultPublishPipeline<T>(T message)
        {
            return 
                from json in PipelineElements.Serialize(message)
                from bytes in PipelineElements.ConvertToByte(json)
                from rawMessage in PipelineElements.CreateMessage(bytes)
                from m2 in PipelineElements.SetMessage(rawMessage)
                from m3 in PipelineElements.SetCorrelationId()
                from m4 in PipelineElements.SetExchangeAndRoutingKey()
                select 0;
        }
    }

    public delegate T ProducerTransformer<T>(MessagePublishingContext context);

    public static class ProducerTransformerExtensions
    {
        public static ProducerTransformer<C> SelectMany<A, B, C>(
                this ProducerTransformer<A> source,
                Func<A, ProducerTransformer<B>> transform,
                Func<A, B, C> select)
        {
            return context =>
            {
                var a = source(context);
                var b = transform(a)(context);
                return select(a, b);
            };
        }
    }

    public class MessagePublishingContext
    {
        public string ContentType { get; set; }
        public string ContentEncoding { get; set; }
        public Type MessageType { get; set; }
        public IPublishSettings PublishSettings { get; set; }
        public IRawMessage RawMessage { get; set; }
    }
}