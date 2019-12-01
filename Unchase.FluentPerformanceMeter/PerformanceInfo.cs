using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Unchase.FluentPerformanceMeter.Attributes;

namespace Unchase.FluentPerformanceMeter
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

    /// <summary>
    /// Method calls information.
    /// </summary>
    /// <typeparam name="T">Method with type <see cref="MethodInfo"/>.</typeparam>
    [DataContract]
    public class MethodCallInfo<T> where T : MethodInfo
    {
        #region Properties

        /// <summary>
        /// Method information.
        /// </summary>
        /// <remarks>
        /// <see cref="MethodInfo"/>.
        /// </remarks>
        public T Method { get; }

        /// <summary>
        /// Method name.
        /// </summary>
        [DataMember]
        public string MethodName { get; }

        /// <summary>
        /// Method call duration.
        /// </summary>
        [DataMember]
        public TimeSpan Elapsed { get; }

        /// <summary>
        /// Caller name.
        /// </summary>
        [DataMember]
        public string Caller { get; }

        /// <summary>
        /// Method call start date.
        /// </summary>
        [DataMember]
        public DateTime StartTime { get; }

        /// <summary>
        /// Method call end date.
        /// </summary>
        [DataMember]
        public DateTime EndTime { get; }

        /// <summary>
        /// Custom data for a specific method call.
        /// </summary>
        [DataMember]
        public IDictionary<string, object> CustomData { get; }

        /// <summary>
        /// Collection of performance meter steps.
        /// </summary>
        [DataMember]
        public IEnumerable<IPerformanceInfoStepData> Steps { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for <see cref="MethodCallInfo{T}"/>.
        /// </summary>
        public MethodCallInfo() { }

        /// <summary>
        /// Constructor for <see cref="MethodCallInfo{T}"/>.
        /// </summary>
        /// <param name="m"><see cref="Method"/>.</param>
        /// <param name="elapsed">Elapsed time.</param>
        /// <param name="caller"><see cref="Caller"/>.</param>
        /// <param name="ds"><see cref="StartTime"/>.</param>
        /// <param name="customData"><see cref="CustomData"/>.</param>
        /// <param name="steps"><see cref="Steps"/>.</param>
        public MethodCallInfo(T m, TimeSpan elapsed, string caller, DateTime ds, IDictionary<string, object> customData, IEnumerable<IPerformanceInfoStepData> steps)
        {
            Method = m;
            if (m != null)
                MethodName = m.Name;
            Elapsed = elapsed;
            Caller = caller;
            StartTime = ds;
            EndTime = ds + elapsed;
            CustomData = customData;
            Steps = steps;
        }

        #endregion
    }

    /// <summary>
    /// Method calls count information.
    /// </summary>
    /// <typeparam name="T">Method with type <see cref="MethodInfo"/>.</typeparam>
    [DataContract]
    public class MethodCallsCount<T> where T : MethodInfo
    {
        #region Properties

        /// <summary>
        /// Method information.
        /// </summary>
        /// <remarks>
        /// <see cref="MethodInfo"/>.
        /// </remarks>
        public T Method { get; }

        /// <summary>
        /// Method name.
        /// </summary>
        [DataMember]
        public string MethodName { get; }

        /// <summary>
        /// Method calls count.
        /// </summary>
        [DataMember]
        public long CallsCount { get; internal set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for <see cref="MethodCallsCount{T}"/>.
        /// </summary>
        public MethodCallsCount() { }

        /// <summary>
        /// Constructor for <see cref="MethodCallsCount{T}"/>.
        /// </summary>
        /// <param name="m"><see cref="Method"/>.</param>
        public MethodCallsCount(T m)
        {
            Method = m;
            if (m != null)
                MethodName = m.Name;
        }

        #endregion
    }
}
