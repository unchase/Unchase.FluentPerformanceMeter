using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Unchase.FluentPerformanceMeter.Models
{
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
        internal MethodCallInfo() { }

        /// <summary>
        /// Constructor for <see cref="MethodCallInfo{T}"/>.
        /// </summary>
        /// <param name="m"><see cref="Method"/>.</param>
        /// <param name="elapsed">Elapsed time.</param>
        /// <param name="caller"><see cref="Caller"/>.</param>
        /// <param name="ds"><see cref="StartTime"/>.</param>
        /// <param name="customData"><see cref="CustomData"/>.</param>
        /// <param name="steps"><see cref="Steps"/>.</param>
        internal MethodCallInfo(T m, TimeSpan elapsed, string caller, DateTime ds, IDictionary<string, object> customData, IEnumerable<IPerformanceInfoStepData> steps)
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
}
