using System.Collections;
using System.Collections.Generic;

namespace EasyNetQ.AMQP
{
    public class Arguments : Dictionary<string, string>
    {
        public IDictionary ToLegacyDictionary()
        {
            var dictionary = new Hashtable();
            foreach (var keyValuePair in this)
            {
                dictionary.Add(keyValuePair.Key, keyValuePair.Value);
            }
            return dictionary;
        } 
    }
}