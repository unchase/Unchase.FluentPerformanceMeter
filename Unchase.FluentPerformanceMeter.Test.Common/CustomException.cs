using System;

namespace Unchase.FluentPerformanceMeter.Test.Common
{
    /// <summary>
    /// Custom exception.
    /// </summary>
    public class CustomException : Exception
    {
        public CustomException(string message) : base(message) { }

        public CustomException(string message, Exception innerException) : base(message, innerException) { }

        public CustomException() { }
    }
}
