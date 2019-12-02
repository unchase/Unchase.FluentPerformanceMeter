using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Unchase.FluentPerformanceMeter.Attributes;

namespace Unchase.FluentPerformanceMeter.Models
{
    /// <summary>
    /// Method performance information.
    /// </summary>
    /// <typeparam name="TClass">Class with public methods.</typeparam>
    [DataContract]
    internal class PerformanceInfo<TClass> : IPerformanceInfo where TClass : class
    {
        #region Properties

        /// <summary>
        /// List of method calls information.
        /// </summary>
        /// <remarks>
        /// <see cref="MethodCallInfo{MethodInfo}"/>.
        /// </remarks>
        [DataMember]
        public List<MethodCallInfo<MethodInfo>> MethodCalls { get; }

        /// <summary>
        /// List of total method calls count information.
        /// </summary>
        /// <remarks>
        /// <see cref="MethodCallsCount{MethodInfo}"/>.
        /// </remarks>
        [DataMember]
        public List<MethodCallsCount<MethodInfo>> TotalActivity { get; }

        /// <summary>
        /// List of current method calls count information.
        /// </summary>
        /// <remarks>
        /// <see cref="MethodCallsCount{MethodInfo}"/>.
        /// </remarks>
        [DataMember]
        public List<MethodCallsCount<MethodInfo>> CurrentActivity { get; }

        /// <summary>
        /// Uptime.
        /// </summary>
        [DataMember]
        public DateTime UptimeSince { get; }

        /// <summary>
        /// Class name.
        /// </summary>
        [DataMember]
        public string ClassName { get; }

        /// <summary>
        /// List of method names.
        /// </summary>
        [DataMember]
        public List<string> MethodNames { get; }

        /// <summary>
        /// Custom data.
        /// </summary>
        [DataMember]
        public IDictionary<string, object> CustomData { get; }

        /// <summary>
        /// Frequency of the timer as the number of ticks per second.
        /// </summary>
        [DataMember]
        public long TimerFrequency => Stopwatch.Frequency;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for <see cref="PerformanceInfo{TClass}"/>.
        /// </summary>
        public PerformanceInfo()
        {
            UptimeSince = DateTime.UtcNow;
            MethodCalls = new List<MethodCallInfo<MethodInfo>>();
            TotalActivity = new List<MethodCallsCount<MethodInfo>>();
            CurrentActivity = new List<MethodCallsCount<MethodInfo>>();
            var methodInfos = typeof(TClass)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(mi => !mi.IsSpecialName && mi.GetCustomAttribute<IgnoreMethodPerformanceAttribute>() == null)
                .ToArray();
            MethodNames = methodInfos.Select(mi => mi.Name).Distinct().ToList();
            foreach (var method in methodInfos)
            {
                TotalActivity.Add(new MethodCallsCount<MethodInfo>(method));
                CurrentActivity.Add(new MethodCallsCount<MethodInfo>(method));
            }
            ClassName = typeof(TClass).FullName;
            CustomData = new Dictionary<string, object>();
        }

        #endregion
    }
}
