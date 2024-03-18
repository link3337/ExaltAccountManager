using System.Runtime.Serialization;

namespace ExaltAccountManager.Core.Exceptions
{
    public class ExaltExeNotFoundException : Exception
    {
        public ExaltExeNotFoundException()
        {
        }

        public ExaltExeNotFoundException(string? message) : base(message)
        {
        }

        public ExaltExeNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ExaltExeNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
