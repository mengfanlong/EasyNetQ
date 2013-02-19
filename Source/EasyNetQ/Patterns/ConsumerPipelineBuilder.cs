using System;
using EasyNetQ.AMQP;

namespace EasyNetQ.Patterns
{
    public interface IConsumerPipelineBuilder
    {
        ConsumerTransformer<T> CreatePipeline<T>();
    }

    public class ConsumerPipelineBuilder : IConsumerPipelineBuilder
    {
        public virtual ConsumerTransformer<T> CreatePipeline<T>()
        {
            return from s in PipelineElements.ConvertToString()
                   from t in PipelineElements.DeSerialize<T>(s)
                   select t;
        }
    }

    public delegate T ConsumerTransformer<T>(IMessageDeliveryContext context);

    public static class ConsumerTransformerExtensions
    {
        public static ConsumerTransformer<C> SelectMany<A, B, C>(
            this ConsumerTransformer<A> source,
            Func<A, ConsumerTransformer<B>> transformer,
            Func<A, B, C> select)
        {
            return context =>
            {
                var a = source(context);
                var b = transformer(a)(context);
                return select(a, b);
            };
        }
    }
}