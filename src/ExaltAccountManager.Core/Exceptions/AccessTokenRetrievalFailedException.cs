using System;
using System.Runtime.Serialization;

namespace ExaltAccountManager.Core.Exceptions
{
    public class AccessTokenRetrievalFailedException : Exception
    {
        public AccessTokenRetrievalFailedException()
        {
        }

        public AccessTokenRetrievalFailedException(string? message) : base(message)
        {
        }

        public AccessTokenRetrievalFailedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected AccessTokenRetrievalFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
