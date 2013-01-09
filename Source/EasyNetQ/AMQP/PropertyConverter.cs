using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        /// <summary>
        /// Convert from EasyNetQ's IMessageProperties to RabbitMQ's IBasicProperties
        /// </summary>
        /// <param name="messageProperties"></param>
        /// <param name="basicProperties"></param>
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

            MapProperties(messageProperties, basicProperties, true);
        }

        /// <summary>
        /// Convert from RabbitMQ's IBasicProperties to EasyNetQ's IMessageProperties
        /// </summary>
        /// <param name="basicProperties"></param>
        /// <returns></returns>
        public static IMessageProperties ConvertFromBasicProperties(IBasicProperties basicProperties)
        {
            if(basicProperties == null)
            {
                throw new ArgumentNullException("basicProperties");
            }

            var messageProperties = new MessageProperties();

            MapProperties(messageProperties, basicProperties, false);

            return messageProperties;
        }


        /// <summary>
        /// Map properties of IMessageProperties and IBasicProperties
        /// </summary>
        /// <param name="messageProperties"></param>
        /// <param name="basicProperties"></param>
        /// <param name="easyNetQToRabbitMq">
        /// If true, map from IMessageProperties to IBasicProperites.
        /// If false, map from IBasicProperties to IMessageProperties
        /// </param>
        private static void MapProperties(
            IMessageProperties messageProperties, 
            IBasicProperties basicProperties,
            bool easyNetQToRabbitMq)
        {
            var easyNetQPropertyInfos =
                from propertyInfo in messageProperties.GetType().GetProperties()
                where propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(PropertyValue<>)
                select propertyInfo;

            foreach (var easyNetQPropertyInfo in easyNetQPropertyInfos)
            {
                var rabbitMqPropertyInfo = basicProperties.GetType().GetProperty(easyNetQPropertyInfo.Name);

                if(!PropertyPresent(
                    easyNetQPropertyInfo, 
                    rabbitMqPropertyInfo, 
                    messageProperties, 
                    basicProperties, 
                    easyNetQToRabbitMq)) continue;

                var easyNetQType = easyNetQPropertyInfo.PropertyType.GetGenericArguments()[0];
                var rabbitMqType = rabbitMqPropertyInfo.PropertyType;
                var converter = GetConverter(easyNetQType, rabbitMqType);
                var propertyValue = (IPropertyValue) easyNetQPropertyInfo.GetValue(messageProperties, null);

                if (easyNetQToRabbitMq)
                {
                    var easyNetQValue = propertyValue.GetValue();
                    var rabbitMqValue = converter.ConvertFromEasyNetQValue(easyNetQValue);
                    rabbitMqPropertyInfo.SetValue(basicProperties, rabbitMqValue, null);
                }
                else
                {
                    var rabbitMqValue = rabbitMqPropertyInfo.GetValue(basicProperties, null);
                    var easyNetQValue = converter.ConvertFromRabbitMqValue(rabbitMqValue);
                    propertyValue.SetValue(easyNetQValue);
                }
            }
        }

        private static bool PropertyPresent(
            PropertyInfo easyNetQPropertyInfo, 
            PropertyInfo rabbitMqPropertyInfo,
            IMessageProperties messageProperties,
            IBasicProperties basicProperties,
            bool easyNetQToRabbitMq)
        {
            if (easyNetQToRabbitMq)
            {
                return ((IPropertyValue) easyNetQPropertyInfo.GetValue(messageProperties, null)).IsSet;
            }

            var isSetPropertyName = "Is" + rabbitMqPropertyInfo.Name + "Present";
            var setPropertyInfo = typeof(IBasicProperties)
                .GetMethod(isSetPropertyName);

            if (setPropertyInfo == null)
            {
                throw new EasyNetQAmqpException("Can't find property: '{0}'", isSetPropertyName);
            }

            return (bool)setPropertyInfo.Invoke(basicProperties, new object[0]);
        }

        private static IPropertyValueConverter GetConverter(Type easyNetQType, Type rabbitMqType)
        {
            if (easyNetQType != rabbitMqType)
            {
                if (!propertyValueConverters.ContainsKey(easyNetQType))
                {
                    throw new EasyNetQAmqpException("No property converter found for type {0}", easyNetQType.Name);
                }

                return propertyValueConverters[easyNetQType];
            }
            return new DefaultConverter();
        }
    }

    public interface IPropertyValueConverter
    {
        object ConvertFromEasyNetQValue(object source);
        object ConvertFromRabbitMqValue(object rabbitMqValue);
    }

    public class DefaultConverter : IPropertyValueConverter
    {
        public object ConvertFromEasyNetQValue(object source)
        {
            return source;
        }

        public object ConvertFromRabbitMqValue(object rabbitMqValue)
        {
            return rabbitMqValue;
        }
    }

    public class DeliveryModeConverter : IPropertyValueConverter
    {
        public object ConvertFromEasyNetQValue(object source)
        {
            return (Byte) ((DeliveryMode) source);
        }

        public object ConvertFromRabbitMqValue(object rabbitMqValue)
        {
            return (DeliveryMode) ((Byte) rabbitMqValue);
        }
    }

    public class TimestampConverter : IPropertyValueConverter
    {
        private static readonly DateTime unixStartDate = new DateTime(1970, 1, 1);

        public object ConvertFromEasyNetQValue(object source)
        {
            var sourceTime = ((DateTime) source).ToUniversalTime();
            var unixTime = (long)(sourceTime - unixStartDate).TotalSeconds;
            return (new AmqpTimestamp(unixTime));
        }

        public object ConvertFromRabbitMqValue(object rabbitMqValue)
        {
            var unixTime = (AmqpTimestamp) rabbitMqValue;
            return unixStartDate.AddSeconds(unixTime.UnixTime);
        }
    }

    public class HeadersConverter : IPropertyValueConverter
    {
        public object ConvertFromEasyNetQValue(object source)
        {
            var headers = (Headers) source;
            var basicPropertyHeaders = new Hashtable();

            foreach (var header in headers)
            {
                basicPropertyHeaders.Add(header.Key, header.Value);
            }

            return basicPropertyHeaders;
        }

        public object ConvertFromRabbitMqValue(object rabbitMqValue)
        {
            var basicPropertyHeaders = (Hashtable) rabbitMqValue;
            var headers = new Headers();

            foreach (string key in basicPropertyHeaders.Keys)
            {
                headers.Add(key, (string) basicPropertyHeaders[key]);
            }

            return headers;
        }
    }
}