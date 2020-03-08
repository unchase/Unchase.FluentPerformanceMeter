using System;

namespace Unchase.FluentPerformanceMeter.AspNetCore.Mvc.Attributes
{
    /// <summary>
    /// Attribute to indicate that performance watching will be performed with diagnostic source.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class WatchingPerformanceAttribute : Attribute { }
}
