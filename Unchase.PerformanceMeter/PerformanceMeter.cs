using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Unchase.PerformanceMeter
{
    /// <summary>
    /// Class for starting and stopping method performance wathing.
    /// </summary>
    /// <typeparam name="TClass">Class with methods.</typeparam>
    public sealed class PerformanceMeter<TClass> : IDisposable where TClass : class
    {
        #region Fields and Properties

        private IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Method information.
        /// </summary>
        /// <returns>
        /// Returns method information with type <see cref="System.Reflection.MethodInfo"/>.
        /// </returns>
        public MethodInfo MethodInfo { get; }

        private Stopwatch _sw = new Stopwatch();

        private DateTime _dateStart = DateTime.Now;

        private string _caller = "unknown";

        private Action<Exception> _exceptionHandler { get; set; }

        private static readonly object PerformanceMeterLock = new object();

        private static Action<Exception> _defaultExceptionHandler { get; set; }
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

        private static ConcurrentDictionary<string, MethodInfo> _cachedMethodInfos = new ConcurrentDictionary<string, MethodInfo>();

        internal ConcurrentDictionary<string, object> _customData = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// Time in minutes to clear list of the method calls.
        /// </summary>
        /// <remarks>
        /// <see cref="IPerformanceInfo.MethodCalls"/>.
        /// </remarks>
        public int MethodCallsCacheTime => Performance<TClass>.MethodCallsCacheTime;

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
            this._exceptionHandler = DefaultExceptionHandler;
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
        /// Returns an instance of the class with type <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> Watching(MethodInfo method)
        {
            if (!_cachedMethodInfos.Contains(new KeyValuePair<string, MethodInfo>(method.Name, method)))
                _cachedMethodInfos.TryAdd(method.Name, method);

            return new PerformanceMeter<TClass>(method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="methodName">Method name.</param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static PerformanceMeter<TClass> Watching([CallerMemberName] string methodName = null)
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

            return new PerformanceMeter<TClass>(methodInfo);
        }

        #endregion

        #region Additional

        /// <summary>
        /// Set <see cref="IHttpContextAccessor"/> to get the ip address of the caller.
        /// </summary>
        /// <param name="httpContextAccessor"><see cref="IHttpContextAccessor"/>.</param>
        internal void SetHttpContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Set Action to handle exceptions that occur.
        /// </summary>
        /// <param name="exceptionHandler">Action to handle exceptions that occur.</param>
        internal void SetExceptionHandler(Action<Exception> exceptionHandler = null)
        {
            this._exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Set caller name.
        /// </summary>
        /// <param name="caller">Caller name.</param>
        internal void SetCallerAddress(string caller)
        {
            this._caller = caller;
        }

        /// <summary>
        /// Register commands which will be executed after the performance watching is completed.
        /// </summary>
        /// <param name="performanceCommands">Collection of the executed commands.</param>
        internal void RegisterCommands(params IPerformanceCommand[] performanceCommands)
        {
            foreach (var performanceCommand in performanceCommands)
            {
                this.RegisteredCommands.Add(performanceCommand);
            }
        }

        /// <summary>
        /// Add custom data.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        internal void AddMethodCallCustomData(string key, object value)
        {
            this._customData.TryAdd(key, value);
        }

        /// <summary>
        /// Add custom data.
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
        /// Set custom handler to receive methods performance information.
        /// </summary>
        /// <param name="performanceInfo"><see cref="IPerformanceInfo"/>.</param>
        public static void SetCustomPerformanceInfo(IPerformanceInfo performanceInfo)
        {
            Performance<TClass>.SetCustomPerformanceInfo(performanceInfo);
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
        /// Set the time in minutes to clear list of the method calls.
        /// </summary>
        /// <param name="minutes">Time in minutes to clear list of the method calls.</param>
        public static void SetMethodCallsCacheTime(int minutes)
        {
            Performance<TClass>.MethodCallsCacheTime = minutes;
        }

        #endregion

        #region Main

        /// <summary>
        /// Start watching methods performance.
        /// </summary>
        internal void Start()
        {
            try
            {
                this._dateStart = DateTime.Now;
                this._sw = Stopwatch.StartNew();
                Performance<TClass>.Input(this.MethodInfo);
            }
            catch (Exception ex)
            {
                if (this._exceptionHandler != null)
                    this._exceptionHandler(ex);
                else
                    throw ex;
            }
        }

        /// <summary>
        /// Get methods performance information.
        /// </summary>
        /// <returns>
        /// Return method performance information with type <see cref="PerformanceInfo{TClass}"/>.
        /// </returns>
        public static IPerformanceInfo GetPerformanceInfo() => Performance<TClass>.PerformanceInfo;

        /// <summary>
        /// Dispose and stop watching methods performance.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (this.MethodInfo != null && this._sw.IsRunning)
                {
                    this._caller = this._httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? this._caller;
                    var performanceInfo = Performance<TClass>.Output(this._caller, this.MethodInfo, this._sw, this._dateStart, this._customData);

                    foreach (var performanceCommand in this.RegisteredCommands)
                        performanceCommand.Execute(performanceInfo);
                }
            }
            catch (Exception ex) 
            {
                if (this._exceptionHandler != null)
                    this._exceptionHandler(ex);
                else
                    throw ex;
            }
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Extension methods for the <see cref="PerformanceMeter{TClass}"/>
    /// </summary>
    public static class PerformanceMeterExtensions
    {
        #region Extension methods

        /// <summary>
        /// Set <see cref="IHttpContextAccessor"/> to get the ip address of the caller.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="performanceMeter"><see cref="PerformanceMeter{TClass}"/>.</param>
        /// <param name="httpContextAccessor"><see cref="IHttpContextAccessor"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> WithHttpContextAccessor<TClass>(this PerformanceMeter<TClass> performanceMeter, IHttpContextAccessor httpContextAccessor) where TClass : class
        {
            performanceMeter.SetHttpContextAccessor(httpContextAccessor);
            return performanceMeter;
        }

        /// <summary>
        /// Set Action to handle exceptions that occur.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="performanceMeter"><see cref="PerformanceMeter{TClass}"/>.</param>
        /// <param name="exceptionHandler">Action to handle exceptions that occur.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> WithExceptionHandler<TClass>(this PerformanceMeter<TClass> performanceMeter, Action<Exception> exceptionHandler = null) where TClass : class
        {
            performanceMeter.SetExceptionHandler(exceptionHandler);
            return performanceMeter;
        }

        /// <summary>
        /// Register commands which will be executed after the performance watching is completed.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="performanceMeter"><see cref="PerformanceMeter{TClass}"/>.</param>
        /// <param name="performanceCommands">Collection of the executed commands.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> WithExecutingOnComplete<TClass>(this PerformanceMeter<TClass> performanceMeter, params IPerformanceCommand[] performanceCommands) where TClass : class
        {
            performanceMeter.RegisterCommands(performanceCommands);
            return performanceMeter;
        }

        /// <summary>
        /// Set caller name.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="performanceMeter"><see cref="PerformanceMeter{TClass}"/>.</param>
        /// <param name="caller">Caller name.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> WithCaller<TClass>(this PerformanceMeter<TClass> performanceMeter, string caller) where TClass : class
        {
            performanceMeter.SetCallerAddress(caller);
            return performanceMeter;
        }

        /// <summary>
        /// Add custom data.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="performanceMeter"><see cref="PerformanceMeter{TClass}"/>.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> WithCustomData<TClass>(this PerformanceMeter<TClass> performanceMeter, string key, object value) where TClass : class
        {
            performanceMeter.AddMethodCallCustomData(key, value);
            return performanceMeter;
        }

        /// <summary>
        /// Start watching methods performance.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="performanceMeter"><see cref="PerformanceMeter{TClass}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> Start<TClass>(this PerformanceMeter<TClass> performanceMeter) where TClass : class
        {
            performanceMeter.Start();
            return performanceMeter;
        }

        #endregion
    }
}
