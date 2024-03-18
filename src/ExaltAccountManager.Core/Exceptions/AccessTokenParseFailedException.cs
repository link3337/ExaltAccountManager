using System.Runtime.Serialization;

namespace ExaltAccountManager.Core.Exceptions
{
    public class AccessTokenParseFailedException : Exception
    {
        public AccessTokenParseFailedException()
        {
        }

        public AccessTokenParseFailedException(string? message) : base(message)
        {
        }

        public AccessTokenParseFailedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected AccessTokenParseFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
