using System;

namespace Unchase.FluentPerformanceMeter.Attributes
{
    /// <summary>
    /// Attribute to add caller to method performance watching.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MethodCallerAttribute : Attribute
    {
        /// <summary>
        /// Caller.
        /// </summary>
        public string Caller { get; }

        /// <summary>
        /// Constructor for <see cref="MethodCallerAttribute"/>.
        /// </summary>
        /// <param name="caller"><see cref="Caller"/>.</param>
        public MethodCallerAttribute(string caller)
        {
            this.Caller = caller;
        }
    }
}
