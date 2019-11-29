using System;

namespace Unchase.FluentPerformanceMeter.TestWebAPI
{
    /// <summary>
    /// Custom exception.
    /// </summary>
    internal class CustomException : Exception
    {
        public CustomException(string message) : base(message) { }

        public CustomException(string message, Exception innerException) : base(message, innerException) { }

        public CustomException() { }
    }
}
