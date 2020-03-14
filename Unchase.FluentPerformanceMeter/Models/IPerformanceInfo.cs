using System;
using System.Collections.Generic;
using System.Reflection;

namespace Unchase.FluentPerformanceMeter.Models
{
    /// <summary>
    /// Method performance information for specific class.
    /// </summary>
    /// <typeparam name="TClass">Class with public methods.</typeparam>
    public interface IPerformanceInfo<TClass> : IPerformanceInfo where TClass : class { }

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
        List<MethodCallInfo<MethodInfo>> MethodCalls { get; }

        /// <summary>
        /// List of total method calls count information.
        /// </summary>
        /// <remarks>
        /// <see cref="MethodCallsCount{MethodInfo}"/>.
        /// </remarks>
        List<MethodCallsCount<MethodInfo>> TotalActivity { get; }

        /// <summary>
        /// List of current method calls count information.
        /// </summary>
        /// <remarks>
        /// <see cref="MethodCallsCount{MethodInfo}"/>.
        /// </remarks>
        List<MethodCallsCount<MethodInfo>> CurrentActivity { get; }

        /// <summary>
        /// Uptime.
        /// </summary>
        DateTime UptimeSince { get; }

        /// <summary>
        /// Frequency of the timer as the number of ticks per second.
        /// </summary>
        long TimerFrequency { get; }

        /// <summary>
        /// Class name.
        /// </summary>
        string ClassName { get; }

        /// <summary>
        /// List of method names.
        /// </summary>
        List<string> MethodNames { get; }

        /// <summary>
        /// Custom data.
        /// </summary>
        IDictionary<string, object> CustomData { get; }
    }
}
