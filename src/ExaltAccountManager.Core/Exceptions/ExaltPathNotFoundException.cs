using System;
using System.Runtime.Serialization;

namespace ExaltAccountManager.Core.Exceptions
{
    public class ExaltPathNotFoundException : Exception
    {
        public ExaltPathNotFoundException()
        {
        }

        public ExaltPathNotFoundException(string? message) : base(message)
        {
        }

        public ExaltPathNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ExaltPathNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
