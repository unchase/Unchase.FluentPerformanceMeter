using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Unchase.PerformanceMeter
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
        public List<MethodCallInfo<MethodInfo>> MethodCalls { get; set; }

        /// <summary>
        /// List of total method calls count information.
        /// </summary>
        /// <remarks>
        /// <see cref="MethodCallsCount{MethodInfo}"/>.
        /// </remarks>
        [DataMember]
        public List<MethodCallsCount<MethodInfo>> TotalActivity { get; set; }

        /// <summary>
        /// List of current method calls count information.
        /// </summary>
        /// <remarks>
        /// <see cref="MethodCallsCount{MethodInfo}"/>.
        /// </remarks>
        [DataMember]
        public List<MethodCallsCount<MethodInfo>> CurrentActivity { get; set; }

        /// <summary>
        /// Uptime.
        /// </summary>
        [DataMember]
        public DateTime UptimeSince { get; set; }

        /// <summary>
        /// Class name.
        /// </summary>
        [DataMember]
        public string ClassName
        {
            get
            {
                return typeof(TClass).FullName;
            }
            set { }
        }

        /// <summary>
        /// List of method names.
        /// </summary>
        [DataMember]
        public List<string> MethodNames { get; set; }

        /// <summary>
        /// Custom data.
        /// </summary>
        [DataMember]
        public IDictionary<string, object> CustomData { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for <see cref="PerformanceInfo{TClass}"/>.
        /// </summary>
        public PerformanceInfo()
        {
            MethodCalls = new List<MethodCallInfo<MethodInfo>>();
            TotalActivity = new List<MethodCallsCount<MethodInfo>>();
            CurrentActivity = new List<MethodCallsCount<MethodInfo>>();
            UptimeSince = DateTime.Now;
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
        public T Method { get; set; }

        /// <summary>
        /// Method name.
        /// </summary>
        [DataMember]
        public string MethodName { get; set; }

        /// <summary>
        /// Method call duration in milliseconds.
        /// </summary>
        [DataMember]
        public long DurationMiliseconds { get; set; }

        /// <summary>
        /// Caller name.
        /// </summary>
        [DataMember]
        public string Caller { get; set; }

        /// <summary>
        /// Method call start date.
        /// </summary>
        [DataMember]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Method call end date.
        /// </summary>
        [DataMember]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Custom data for a specific method call.
        /// </summary>
        [DataMember]
        public IDictionary<string, object> CustomData { get; set; }

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
        /// <param name="duration"><see cref="DurationMiliseconds"/>.</param>
        /// <param name="caller"><see cref="Caller"/>.</param>
        /// <param name="ds"><see cref="StartTime"/>.</param>
        /// <param name="de"><see cref="EndTime"/>.</param>
        /// <param name="customData"><see cref="CustomData"/>.</param>
        public MethodCallInfo(T m, long duration, string caller, DateTime ds, DateTime de, IDictionary<string, object> customData)
        {
            Method = m;
            if (m != null)
                MethodName = m.Name;
            DurationMiliseconds = duration;
            Caller = caller;
            StartTime = ds;
            EndTime = de;
            CustomData = customData;
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
        public T Method { get; set; }

        /// <summary>
        /// Method name.
        /// </summary>
        [DataMember]
        public string MethodName { get; set; }

        /// <summary>
        /// Method calls count.
        /// </summary>
        [DataMember]
        public long CallsCount { get; set; }

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
