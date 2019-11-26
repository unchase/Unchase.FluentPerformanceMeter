using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Unchase.PerformanceMeter
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
        internal static int MethodCallsCacheTime { get; set; } = 5;

        #endregion

        #region Public methods

        /// <summary>
        /// Set custom handler for receiving data on the performance of methods.
        /// </summary>
        /// <param name="performanceInfo"><see cref="IPerformanceInfo"/>.</param>
        internal static void SetCustomPerformanceInfo(IPerformanceInfo performanceInfo)
        {
            lock (PerformanceLock)
            {
                _performanceInfo = performanceInfo;
            }
        }

        /// <summary>
        /// Start tracking method performance.
        /// </summary>
        /// <param name="method">Method with type <see cref="MethodInfo"/>.</param>
        internal static void Input(MethodInfo method)
        {
            var currentActivity = PerformanceInfo.CurrentActivity.Find(x => x.Method == method);
            if (currentActivity != null)
                currentActivity.CallsCount++;
        }

        /// <summary>
        /// Complete method performance tracking.
        /// </summary>
        /// <param name="caller">Caller name.</param>
        /// <param name="method">Method with type <see cref="MethodInfo"/>.</param>
        /// <param name="sw"><see cref="Stopwatch"/> to track the running time of a method.</param>
        /// <param name="dateStart">Method start date.</param>
        /// <param name="customData">Custom data for a specific method call.</param>
        internal static IPerformanceInfo Output(string caller, MethodInfo method, Stopwatch sw, DateTime dateStart, IDictionary<string, object> customData = null)
        {
            if (method != null && sw.IsRunning)
            {
                sw.Stop();
                var currentActivity = PerformanceInfo.CurrentActivity.Find(x => x.Method == method);
                if (currentActivity != null)
                    currentActivity.CallsCount--;
                if (method.GetCustomAttribute<IgnoreMethodPerformanceAttribute>() == null)
                    PerformanceInfo.MethodCalls.Add(new MethodCallInfo<MethodInfo>(method, sw.ElapsedMilliseconds, caller, dateStart, DateTime.Now, customData));
                var totalActivity = PerformanceInfo.TotalActivity.Find(x => x.Method == method);
                if (totalActivity != null)
                    totalActivity.CallsCount++;
            }
            return PerformanceInfo;
        }

        #endregion
    }
}
