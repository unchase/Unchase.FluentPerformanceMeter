using System;

namespace Unchase.FluentPerformanceMeter.Attributes
{
    /// <summary>
    /// Attribute to add custom data to method performance watching.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class MethodCustomDataAttribute : Attribute
    {
        /// <summary>
        /// Key.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Value.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Constructor for <see cref="MethodCustomDataAttribute"/>.
        /// </summary>
        /// <param name="key"><see cref="Key"/>.</param>
        /// <param name="value"><see cref="Value"/>.</param>
        public MethodCustomDataAttribute(string key, object value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}
