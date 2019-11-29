using System;

namespace Unchase.FluentPerformanceMeter.Attributes
{
    /// <summary>
    /// Attribute to indicate that no performance watching will be performed for this method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class IgnoreMethodPerformanceAttribute : Attribute { }
}
