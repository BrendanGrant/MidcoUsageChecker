using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MidcoUsageChecker
{
    [Serializable]
    internal class NoLongerLoggedInException : Exception
    {
        public NoLongerLoggedInException()
        {
        }

        public NoLongerLoggedInException(string message) : base(message)
        {
        }

        public NoLongerLoggedInException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoLongerLoggedInException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
