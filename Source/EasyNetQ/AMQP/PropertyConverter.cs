using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;

namespace EasyNetQ.AMQP
{
    public class PropertyConverter
    {
        private static readonly IDictionary<Type, IPropertyValueConverter> propertyValueConverters = 
            new Dictionary<Type, IPropertyValueConverter>();

        static PropertyConverter()
        {
            propertyValueConverters.Add(typeof(DeliveryMode), new DeliveryModeConverter());
            propertyValueConverters.Add(typeof(DateTime), new TimestampConverter());
            propertyValueConverters.Add(typeof(Headers), new HeadersConverter());
        }

        public static void ConvertToBasicProperties(IMessageProperties messageProperties, IBasicProperties basicProperties)
        {
            if(messageProperties == null)
            {
                throw new ArgumentNullException("messageProperties");
            }
            if(basicProperties == null)
            {
                throw new ArgumentNullException("basicProperties");
            }

            var propertyInfos =
                from propertyInfo in messageProperties.GetType().GetProperties()
                where propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof (PropertyValue<>)
                where ((IPropertyValue) propertyInfo.GetValue(messageProperties, null)).IsSet
                select propertyInfo;

            foreach (var propertyInfo in propertyInfos)
            {
                var basicPropertyInfo = basicProperties.GetType().GetProperty(propertyInfo.Name);

                var sourceType = propertyInfo.PropertyType.GetGenericArguments()[0];
                var targetType = basicPropertyInfo.PropertyType;

                object sourceValue = null;
                var originalSourceValue = ((IPropertyValue) propertyInfo.GetValue(messageProperties, null)).GetValue();
                if(sourceType != targetType)
                {
                    if (!propertyValueConverters.ContainsKey(sourceType))
                    {
                        throw new EasyNetQAmqpException("No property converter found for type {0}", sourceType.Name);
                    }

                    var converter = propertyValueConverters[sourceType];
                    sourceValue = converter.Convert(originalSourceValue);
                }
                else
                {
                    sourceValue = originalSourceValue;
                }
                basicPropertyInfo.SetValue(basicProperties, sourceValue, null);
            }
        }     
    }

    public interface IPropertyValueConverter
    {
        object Convert(object source);
    }

    public class DeliveryModeConverter : IPropertyValueConverter
    {
        public object Convert(object source)
        {
            return (Byte) ((DeliveryMode) source);
        }
    }

    public class TimestampConverter : IPropertyValueConverter
    {
        public object Convert(object source)
        {
            var sourceTime = ((DateTime) source).ToUniversalTime();
            var unixTime = (long)(sourceTime - new DateTime(1970, 1, 1)).TotalSeconds;
            return (new AmqpTimestamp(unixTime));
        }
    }

    public class HeadersConverter : IPropertyValueConverter
    {
        public object Convert(object source)
        {
            var headers = (Headers) source;
            var basicPropertyHeaders = new Hashtable();

            foreach (var header in headers)
            {
                basicPropertyHeaders.Add(header.Key, header.Value);
            }

            return basicPropertyHeaders;
        }
    }
}