using System;
using System.Collections.Generic;
using System.Reflection;
using Unchase.FluentPerformanceMeter.Attributes;
using Unchase.FluentPerformanceMeter.Models;

namespace Unchase.FluentPerformanceMeter
{
    /// <summary>
    /// Additional class for performance information.
    /// </summary>
    /// <typeparam name="TClass">Class with public methods.</typeparam>
    internal static class Performance<TClass> where TClass : class
    {
        #region Fields

        private static readonly object PerformanceLock = new object();

        private static DateTime _lastRemoveDate;

        private static int _defaultMethodCallsCacheTime = 5;

        #endregion

        #region Properties

        private static IPerformanceInfo _performanceInfo;
        /// <summary>
        /// Methods performance information.
        /// </summary>
        internal static IPerformanceInfo PerformanceInfo
        {
            get
            {
                lock (PerformanceLock)
                {
                    if (_performanceInfo == null)
                    {
                        _performanceInfo = new PerformanceInfo<TClass>();
                        _lastRemoveDate = DateTime.Now;
                    }
                    else
                    {
                        if (_lastRemoveDate.AddMinutes(MethodCallsCacheTime) < DateTime.Now)
                        {
                            _performanceInfo.MethodCalls?.RemoveAll(x => x?.StartTime.CompareTo(DateTime.Now.AddMinutes(-MethodCallsCacheTime)) < 0);
                            _lastRemoveDate = DateTime.Now;
                        }
                    }
                    return _performanceInfo;
                }
            }
        }

        /// <summary>
        /// Time in minutes to clear list of the method calls.
        /// </summary>
        /// <remarks>
        /// <see cref="IPerformanceInfo.MethodCalls"/>.
        /// </remarks>
        internal static int MethodCallsCacheTime { get; set; } = _defaultMethodCallsCacheTime;

        #endregion

        #region Public methods

        /// <summary>
        /// Start watching method performance.
        /// </summary>
        /// <param name="method">Method with type <see cref="MethodInfo"/>.</param>
        internal static void Input(MethodInfo method)
        {
            var currentActivity = PerformanceInfo.CurrentActivity.Find(x => x.Method == method);
            if (currentActivity != null)
                currentActivity.CallsCount++;
        }

        /// <summary>
        /// Stop watching method performance.
        /// </summary>
        /// <param name="caller">Caller name.</param>
        /// <param name="method">Method with type <see cref="MethodInfo"/>.</param>
        /// <param name="elapsed">Elapsed time from the running of a method.</param>
        /// <param name="dateStart">Method start date.</param>
        /// <param name="customData">Custom data for a specific method call.</param>
        /// <param name="steps">Performance meter steps.</param>
        internal static IPerformanceInfo Output(string caller, MethodInfo method, TimeSpan elapsed, DateTime dateStart, IDictionary<string, object> customData = null, IEnumerable<IPerformanceInfoStepData> steps = null)
        {
            var currentActivity = PerformanceInfo.CurrentActivity.Find(x => x.Method == method);
            if (currentActivity != null)
                currentActivity.CallsCount--;
            if (method.GetCustomAttribute<IgnoreMethodPerformanceAttribute>() == null)
                PerformanceInfo.MethodCalls.Add(new MethodCallInfo<MethodInfo>(method, elapsed, caller, dateStart, customData, steps));
            var totalActivity = PerformanceInfo.TotalActivity.Find(x => x.Method == method);
            if (totalActivity != null)
                totalActivity.CallsCount++;
            return PerformanceInfo;
        }

        /// <summary>
        /// Clear all performance watching information.
        /// </summary>
        /// <returns>
        /// Returns <see cref="IPerformanceInfo"/>.
        /// </returns>
        internal static IPerformanceInfo Reset()
        {
            _performanceInfo = null;
            MethodCallsCacheTime = _defaultMethodCallsCacheTime;
            return PerformanceInfo;
        }

        #endregion
    }
}
