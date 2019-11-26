using System;
using System.Collections.Generic;
using System.Reflection;

namespace Unchase.PerformanceMeter
{
    /// <summary>
    /// Method performance information.
    /// </summary>
    public interface IPerformanceInfo
    {
        /// <summary>
        /// List of method calls information.
        /// </summary>
        /// <remarks>
        /// <see cref="MethodCallInfo{MethodInfo}"/>.
        /// </remarks>
        List<MethodCallInfo<MethodInfo>> MethodCalls { get; set; }

        /// <summary>
        /// List of total method calls count information.
        /// </summary>
        /// <remarks>
        /// <see cref="MethodCallsCount{MethodInfo}"/>.
        /// </remarks>
        List<MethodCallsCount<MethodInfo>> TotalActivity { get; set; }

        /// <summary>
        /// List of current method calls count information.
        /// </summary>
        /// <remarks>
        /// <see cref="MethodCallsCount{MethodInfo}"/>.
        /// </remarks>
        List<MethodCallsCount<MethodInfo>> CurrentActivity { get; set; }

        /// <summary>
        /// Uptime.
        /// </summary>
        DateTime UptimeSince { get; set; }

        /// <summary>
        /// Class name.
        /// </summary>
        string ClassName { get; set; }

        /// <summary>
        /// List of method names.
        /// </summary>
        List<string> MethodNames { get; set; }

        /// <summary>
        /// Custom data.
        /// </summary>
        IDictionary<string, object> CustomData { get; set; }
    }
}
