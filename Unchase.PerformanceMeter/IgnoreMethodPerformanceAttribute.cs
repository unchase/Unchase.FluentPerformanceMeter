using System;

namespace Unchase.PerformanceMeter
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class IgnoreMethodPerformanceAttribute : Attribute { }
}
