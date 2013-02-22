using System;
using System.Runtime.Serialization;

namespace EasyNetQ.AMQP
{
    [Serializable]
    public class EasyNetQOpenChannelException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public EasyNetQOpenChannelException()
        {
        }

        public EasyNetQOpenChannelException(string message) : base(message)
        {
        }

        public EasyNetQOpenChannelException(string message, Exception inner) : base(message, inner)
        {
        }

        protected EasyNetQOpenChannelException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}