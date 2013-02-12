using System;
using System.Runtime.Serialization;

namespace EasyNetQ.AMQP
{
    [Serializable]
    public class EasyNetQPublishException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public EasyNetQPublishException()
        {
        }

        public EasyNetQPublishException(string message) : base(message)
        {
        }

        public EasyNetQPublishException(string format, params object[] args) : base(string.Format(format, args))
        {
        }

        public EasyNetQPublishException(string message, Exception inner) : base(message, inner)
        {
        }

        protected EasyNetQPublishException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}