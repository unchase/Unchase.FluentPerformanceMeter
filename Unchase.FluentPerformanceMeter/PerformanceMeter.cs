using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unchase.FluentPerformanceMeter.Builders;
using Unchase.FluentPerformanceMeter.Models;

namespace Unchase.FluentPerformanceMeter
{
    /// <summary>
    /// Class for starting and stopping method performance watching.
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

        // Track whether Dispose has been called.
        private bool _disposed;

        private static ConcurrentDictionary<string, MethodInfo> _cachedMethodInfos = new ConcurrentDictionary<string, MethodInfo>();

        internal ConcurrentDictionary<string, object> CustomData = new ConcurrentDictionary<string, object>();

        internal IHttpContextAccessor HttpContextAccessor { get; set; }

        internal Stopwatch InnerStopwatch { get; set; } = new Stopwatch();

        internal DateTime DateStart { get; set; } = DateTime.UtcNow;

        internal string Caller { get; set; } = "unknown";

        internal List<IPerformanceInfoStepData> Steps { get; set; } = new List<IPerformanceInfoStepData>();

        /// <summary>
        /// Action to handle exceptions that occur.
        /// </summary>
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

        private Collection<Action<IPerformanceInfo>> _registeredActions;
        /// <summary>
        /// Collection of registered executed actions.
        /// </summary>
        /// <remarks>
        /// <see cref="IPerformanceCommand"/>.
        /// </remarks>
        internal Collection<Action<IPerformanceInfo>> RegisteredActions
        {
            get
            {
                if (this._registeredActions == null)
                {
                    this._registeredActions = new Collection<Action<IPerformanceInfo>>();
                }
                return this._registeredActions;
            }
        }

        #endregion

        #region Constructors and destructor

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

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        /// <summary>
        /// Destructor for <see cref="PerformanceMeter{TClass}"/>.
        /// </summary>
        ~PerformanceMeter()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion

        #region Methods

        #region WatchingMethod

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
        /// Get and remove custom data from performance meter information.
        /// </summary>
        /// <typeparam name="TResult">Type of custom data result.</typeparam>
        /// <param name="key">Key.</param>
        /// <returns>
        /// Returns typed result.
        /// </returns>
        public TResult GetAndRemoveCustomData<TResult>(string key)
        {
            if (this.CustomData.ContainsKey(key))
            {
                if (this.CustomData.TryRemove(key, out var result) && result is TResult typedResult)
                {
                    return typedResult;
                }
            }
            return default;
        }

        /// <summary>
        /// Remove common custom data of the class.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>
        /// Returns typed result.
        /// </returns>
        internal static void RemoveCustomData(string key)
        {
            lock (PerformanceMeterLock)
            {
                if (Performance<TClass>.PerformanceInfo.CustomData.ContainsKey(key))
                {
                    Performance<TClass>.PerformanceInfo.CustomData.Remove(key);
                }
            }
        }

        /// <summary>
        /// Clear common custom data of the class.
        /// </summary>
        public static void ClearCustomData()
        {
            lock (PerformanceMeterLock)
            {
                Performance<TClass>.PerformanceInfo.CustomData.Clear();
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
        /// Print performance information for this class.
        /// </summary>
        /// <returns>
        /// Returns performance information for this class.
        /// </returns>
        public static string Print()
        {
            var performanceInfo = PerformanceInfo;
            var text = new StringBuilder();
            text.AppendLine($"Performance information for class \"{performanceInfo.ClassName}\"");
            text.AppendLine();
            text.AppendLine();
            text.AppendLine("Current activity:");
            text.AppendLine();
            text.AppendFormat("{0, 6} | {1}{2}", "Calls", "Method name", Environment.NewLine);
            foreach (var currentActivity in performanceInfo.CurrentActivity)
                text.AppendFormat("{0, 6} | {1}{2}", currentActivity.CallsCount, currentActivity.MethodName, Environment.NewLine);

            text.AppendLine();
            text.AppendLine("Total activity:");
            text.AppendLine();
            text.AppendFormat("{0, 6} | {1}{2}", "Calls", "Method name", Environment.NewLine);
            foreach (var totalActivity in performanceInfo.TotalActivity)
                text.AppendFormat("{0, 6} | {1}{2}", totalActivity.CallsCount, totalActivity.MethodName, Environment.NewLine);

            text.AppendLine();
            text.AppendLine("Method calls:");
            text.AppendLine();

            foreach (var methodCalls in performanceInfo.MethodCalls.OrderBy(mc => mc.StartTime))
                text.AppendLine($"\t{methodCalls.MethodName}:");

            return text.ToString();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="methodName">Method name.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static PerformanceMeter<TClass> StartWatching([CallerMemberName] string methodName = null)
        {
            return WatchingMethod(methodName).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="method">Method with type <see cref="System.Reflection.MethodInfo"/>.</param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching(MethodInfo method)
        {
            return WatchingMethod(method).Start();
        }

        /// <summary>
        /// Clear all performance watching information.
        /// </summary>
        /// <returns>
        /// Returns <see cref="IPerformanceInfo"/>.
        /// </returns>
        public static IPerformanceInfo Reset()
        {
            DefaultExceptionHandler = (ex) => { AddCustomData("Last exception", ex); };
            return Performance<TClass>.Reset();
        }

        /// <summary>
        /// Stop watching method performance.
        /// </summary>
        public void StopWatching()
        {
            this.Dispose();
        }

        /// <summary>
        /// Implement IDisposable.
        /// </summary>
        /// <remarks>
        /// Stop watching method performance.
        /// </remarks>
        public void Dispose()
        {
            try
            {
                Dispose(true);
                // This object will be cleaned up by the Dispose method.
                // Therefore, you should call GC.SupressFinalize to
                // take this object off the finalization queue
                // and prevent finalization code for this object
                // from executing a second time.
                GC.SuppressFinalize(this);
            }
            catch (Exception ex)
            {
                if (this.ExceptionHandler != null)
                    this.ExceptionHandler(ex);
                else
                    throw;
            }
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._disposed)
            {
                if (disposing)
                {
                    if (this.MethodInfo != null && this.InnerStopwatch?.IsRunning == true)
                    {
                        this.InnerStopwatch.Stop();
                        this.Caller = this.HttpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? this.Caller;
                        var performanceInfo = Performance<TClass>.Output(this.Caller, this.MethodInfo, this.InnerStopwatch.Elapsed, this.DateStart, this.CustomData, this.Steps);
                        this.InnerStopwatch = null;
                        foreach (var performanceCommand in this.RegisteredCommands)
                            performanceCommand.Execute(performanceInfo);

                        foreach (var performanceAction in this.RegisteredActions)
                            performanceAction(PerformanceInfo);
                    }
                }
            }
            this._disposed = true;
        }

        #endregion

        #region Executing

        /// <summary>
        /// Allows execute configured Action or Func.
        /// </summary>
        /// <typeparam name="TException"></typeparam>
        /// <returns>
        /// Returns <see cref="CodeExecutorBuilder{TClass, TException}"/>.
        /// </returns>
        public CodeExecutorBuilder<TClass, TException> Executing<TException>() where TException : Exception
        {
            return new CodeExecutorBuilder<TClass, TException>(this);
        }

        /// <summary>
        /// Allows execute configured Action or Func.
        /// </summary>
        /// <returns>
        /// Returns <see cref="CodeExecutorBuilder{TClass, TException}"/>.
        /// </returns>
        public CodeExecutorBuilder<TClass, Exception> Executing()
        {
            return new CodeExecutorBuilder<TClass, Exception>(this);
        }

        #endregion

        #region Inline

        /// <summary>
        /// Execute the Action.
        /// </summary>
        /// <param name="action">Executed Action.</param>
        public void Inline(Action action)
        {
            new CodeExecutorBuilder<TClass, Exception>(this).Start(action);
        }

        /// <summary>
        /// Execute the Task.
        /// </summary>
        /// <param name="task">Executed Task.</param>
        /// <returns>
        /// Returns <see cref="Task"/>.
        /// </returns>
        public async Task InlineAsync(Task task)
        {
            await new CodeExecutorBuilder<TClass, Exception>(this).StartAsync(task);
        }

        /// <summary>
        /// Execute the Func.
        /// </summary>
        /// <typeparam name="TResult">Type of result.</typeparam>
        /// <param name="func">Executed Func.</param>
        /// <param name="defaultResult">Default result if exception will occured.</param>
        /// <returns>
        /// Returns result.
        /// </returns>
        public TResult Inline<TResult>(Func<TResult> func, TResult defaultResult = default)
        {
            return new CodeExecutorBuilder<TClass, Exception>(this).Start(func, defaultResult);
        }

        /// <summary>
        /// Execute the Func.
        /// </summary>
        /// <typeparam name="TResult">Type of result.</typeparam>
        /// <param name="task">Executed Task.</param>
        /// <param name="defaultResult">Default result if exception will occured.</param>
        /// <returns>
        /// Resturns Task of result.
        /// </returns>
        public async Task<TResult> InlineAsync<TResult>(Task<TResult> task, TResult defaultResult = default)
        {
            return await new CodeExecutorBuilder<TClass, Exception>(this).StartAsync(task, defaultResult);
        }

        #endregion

        #region InlineIgnored

        /// <summary>
        /// Execute the Action.
        /// </summary>
        /// <param name="action">Executed Action.</param>
        public void InlineIgnored(Action action)
        {
            new CodeExecutorBuilder<TClass, Exception>(this).WithoutWatching().Start(action);
        }

        /// <summary>
        /// Execute the Task.
        /// </summary>
        /// <param name="task">Executed Task.</param>
        /// <returns>
        /// Returns <see cref="Task"/>.
        /// </returns>
        public async Task InlineIgnoredAsync(Task task)
        {
            await new CodeExecutorBuilder<TClass, Exception>(this).WithoutWatching().StartAsync(task);
        }

        /// <summary>
        /// Execute the Func.
        /// </summary>
        /// <typeparam name="TResult">Type of result.</typeparam>
        /// <param name="func">Executed Func.</param>
        /// <param name="defaultResult">Default result if exception will occured.</param>
        /// <returns>
        /// Returns result.
        /// </returns>
        public TResult InlineIgnored<TResult>(Func<TResult> func, TResult defaultResult = default)
        {
            return new CodeExecutorBuilder<TClass, Exception>(this).WithoutWatching().Start(func, defaultResult);
        }

        /// <summary>
        /// Execute the Func.
        /// </summary>
        /// <typeparam name="TResult">Type of result.</typeparam>
        /// <param name="task">Executed Task.</param>
        /// <param name="defaultResult">Default result if exception will occured.</param>
        /// <returns>
        /// Resturns Task of result.
        /// </returns>
        public async Task<TResult> InlineIgnoredAsync<TResult>(Task<TResult> task, TResult defaultResult = default)
        {
            return await new CodeExecutorBuilder<TClass, Exception>(this).WithoutWatching().StartAsync(task, defaultResult);
        }

        #endregion

        #endregion
    }
}
