using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unchase.PerformanceMeter.Builders;

namespace Unchase.PerformanceMeter
{
    /// <summary>
    /// Class for starting and stopping method performance wathing.
    /// </summary>
    /// <typeparam name="TClass">Class with methods.</typeparam>
    public sealed class PerformanceMeter<TClass> : IDisposable where TClass : class
    {
        #region Public properties

        /// <summary>
        /// Method information.
        /// </summary>
        /// <returns>
        /// Returns method information with type <see cref="System.Reflection.MethodInfo"/>.
        /// </returns>
        public MethodInfo MethodInfo { get; }

        /// <summary>
        /// Methods performance information.
        /// </summary>
        /// <returns>
        /// Return method performance information with type <see cref="PerformanceInfo{TClass}"/>.
        /// </returns>
        public static IPerformanceInfo PerformanceInfo => Performance<TClass>.PerformanceInfo;

        /// <summary>
        /// Time in minutes to clear list of the method calls.
        /// </summary>
        /// <remarks>
        /// <see cref="IPerformanceInfo.MethodCalls"/>.
        /// </remarks>
        public int MethodCallsCacheTime => Performance<TClass>.MethodCallsCacheTime;

        #endregion

        #region Other properties and fields

        private static ConcurrentDictionary<string, MethodInfo> _cachedMethodInfos = new ConcurrentDictionary<string, MethodInfo>();

        internal ConcurrentDictionary<string, object> CustomData { get; set; } = new ConcurrentDictionary<string, object>();

        internal IHttpContextAccessor HttpContextAccessor { get; set; }

        internal Stopwatch Sw { get; set; } = new Stopwatch();

        internal DateTime DateStart { get; set; } = DateTime.Now;

        internal string Caller { get; set; } = "unknown";

        internal Action<Exception> ExceptionHandler { get; set; }

        internal static readonly object PerformanceMeterLock = new object();

        private static Action<Exception> _defaultExceptionHandler { get; set; } = (ex) => { AddCustomData("Last exception", ex); };
        private static Action<Exception> DefaultExceptionHandler
        {
            get
            {
                lock (PerformanceMeterLock)
                {
                    return _defaultExceptionHandler;
                }
            }
            set
            {
                lock (PerformanceMeterLock)
                {
                    _defaultExceptionHandler = value;
                }
            }
        }

        private Collection<IPerformanceCommand> _registeredComands;
        /// <summary>
        /// Collection of registered executed commands.
        /// </summary>
        /// <remarks>
        /// <see cref="IPerformanceCommand"/>.
        /// </remarks>
        internal Collection<IPerformanceCommand> RegisteredCommands
        {
            get
            {
                if (this._registeredComands == null)
                {
                    this._registeredComands = new Collection<IPerformanceCommand>();
                }
                return this._registeredComands;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Private constructor for <see cref="PerformanceMeter{TClass}"/>.
        /// </summary>
        private PerformanceMeter(MethodInfo method)
        {
            this.MethodInfo = method;
            this.ExceptionHandler = DefaultExceptionHandler;
        }

        /// <summary>
        /// Static constructor for <see cref="PerformanceMeter{TClass}"/>.
        /// </summary>
        static PerformanceMeter() { }

        #endregion

        #region Methods

        #region Watching

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="method">Method with type <see cref="System.Reflection.MethodInfo"/>.</param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod(MethodInfo method)
        {
            if (!_cachedMethodInfos.Contains(new KeyValuePair<string, MethodInfo>(method.Name, method)))
                _cachedMethodInfos.TryAdd(method.Name, method);

            return new PerformanceMeterBuilder<TClass>(new PerformanceMeter<TClass>(method));
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="methodName">Method name.</param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static PerformanceMeterBuilder<TClass> WatchingMethod(
            [CallerMemberName] string methodName = null)
        {
            MethodInfo methodInfo;
            if (_cachedMethodInfos.ContainsKey(methodName))
                methodInfo = _cachedMethodInfos[methodName];
            else
            {
                methodInfo = typeof(TClass)
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                    .FirstOrDefault(m => m.Name == methodName);
                _cachedMethodInfos.TryAdd(methodName, methodInfo);
            }
            return new PerformanceMeterBuilder<TClass>(new PerformanceMeter<TClass>(methodInfo));
        }

        #endregion

        #region Additional

        /// <summary>
        /// Add common custom data of the class.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public static void AddCustomData(string key, object value)
        {
            lock(PerformanceMeterLock)
            {
                if (!Performance<TClass>.PerformanceInfo.CustomData.ContainsKey(key))
                    Performance<TClass>.PerformanceInfo.CustomData.Add(key, value);
                else
                    Performance<TClass>.PerformanceInfo.CustomData[key] = value;
            }
        }

        /// <summary>
        /// Set Action to handle exceptions that occur by default.
        /// </summary>
        /// <param name="exceptionHandler">Action to handle exceptions that occur.</param>
        public static void SetDefaultExceptionHandler(Action<Exception> exceptionHandler = null)
        {
            DefaultExceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Set the time in minutes to clear collection of the method calls.
        /// </summary>
        /// <param name="minutes">Time in minutes to clear list of the method calls.</param>
        public static void SetMethodCallsCacheTime(int minutes)
        {
            Performance<TClass>.MethodCallsCacheTime = minutes;
        }

        #endregion

        #region Main

        /// <summary>
        /// Stop watching method performance.
        /// </summary>
        public void StopWatching()
        {
            this.Dispose();
        }

        /// <summary>
        /// Dispose and stop watching method performance.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (this.MethodInfo != null && this.Sw.IsRunning)
                {
                    this.Caller = this.HttpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? this.Caller;
                    var performanceInfo = Performance<TClass>.Output(this.Caller, this.MethodInfo, this.Sw, this.DateStart, this.CustomData);

                    foreach (var performanceCommand in this.RegisteredCommands)
                        performanceCommand.Execute(performanceInfo);
                }
            }
            catch (Exception ex) 
            {
                if (this.ExceptionHandler != null)
                    this.ExceptionHandler(ex);
                else
                    throw ex;
            }
        }

        #endregion

        #endregion
    }
}
