﻿using Microsoft.AspNetCore.Http;
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
        public static IPerformanceInfo<TClass> PerformanceInfo => Performance<TClass>.PerformanceInfo;

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

        private Collection<IPerformanceCommand> _registeredCommands;
        /// <summary>
        /// Collection of registered executed commands.
        /// </summary>
        /// <remarks>
        /// <see cref="IPerformanceCommand"/>.
        /// </remarks>
        internal Collection<IPerformanceCommand> RegisteredCommands => this._registeredCommands ?? (this._registeredCommands = new Collection<IPerformanceCommand>());

        private Collection<Action<IPerformanceInfo>> _registeredActions;
        /// <summary>
        /// Collection of registered executed actions.
        /// </summary>
        /// <remarks>
        /// <see cref="IPerformanceCommand"/>.
        /// </remarks>
        internal Collection<Action<IPerformanceInfo>> RegisteredActions =>
            this._registeredActions ??
            (this._registeredActions = new Collection<Action<IPerformanceInfo>>());

        /// <summary>
        /// Settings for context-less settings access.
        /// For example, every <see cref="PerformanceMeter{TClass}"/> deserialized from a store would have these settings.
        /// </summary>
        internal static PerformanceMeterBaseOptions DefaultOptions { get; private set; } = new PerformanceMeterBaseOptions();

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

        #region Configure options

        /// <summary>
        /// Saves the given <paramref name="options"/> as the global <see cref="DefaultOptions"/> available for use globally.
        /// These are intended to be used by global/background operations where normal context access isn't available.
        /// </summary>
        /// <typeparam name="T">The specific type of <see cref="PerformanceMeterBaseOptions"/> to use.</typeparam>
        /// <param name="options">The options object to set for background access.</param>
        public static T Configure<T>(T options) where T : PerformanceMeterBaseOptions
        {
            DefaultOptions = options ?? throw new ArgumentNullException(nameof(options));
            options.Configure(); // Event handler of sorts
            return options;
        }

        #endregion

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

        #region With Action

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod(Action action)
        {
            return WatchingMethod(action.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T>(Action<T> action)
        {
            return WatchingMethod(action.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2>(Action<T1, T2> action)
        {
            return WatchingMethod(action.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3>(Action<T1, T2, T3> action)
        {
            return WatchingMethod(action.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action)
        {
            return WatchingMethod(action.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action)
        {
            return WatchingMethod(action.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action)
        {
            return WatchingMethod(action.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6, T7}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action)
        {
            return WatchingMethod(action.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6, T7, T8}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action)
        {
            return WatchingMethod(action.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6, T7, T8, T9}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action)
        {
            return WatchingMethod(action.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action)
        {
            return WatchingMethod(action.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action)
        {
            return WatchingMethod(action.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action)
        {
            return WatchingMethod(action.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action)
        {
            return WatchingMethod(action.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action)
        {
            return WatchingMethod(action.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action)
        {
            return WatchingMethod(action.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action)
        {
            return WatchingMethod(action.Method);
        }

        #endregion

        #region With Func

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{TResult}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<TResult>(Func<TResult> func)
        {
            return WatchingMethod(func.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T, TResult}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T, TResult>(Func<T, TResult> func)
        {
            return WatchingMethod(func.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, TResult}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, TResult>(Func<T1, T2, TResult> func)
        {
            return WatchingMethod(func.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, TResult}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func)
        {
            return WatchingMethod(func.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, TResult}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func)
        {
            return WatchingMethod(func.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, TResult}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func)
        {
            return WatchingMethod(func.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, TResult}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func)
        {
            return WatchingMethod(func.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, T7, TResult}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> func)
        {
            return WatchingMethod(func.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, TResult}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func)
        {
            return WatchingMethod(func.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> func)
        {
            return WatchingMethod(func.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> func)
        {
            return WatchingMethod(func.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> func)
        {
            return WatchingMethod(func.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> func)
        {
            return WatchingMethod(func.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> func)
        {
            return WatchingMethod(func.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> func)
        {
            return WatchingMethod(func.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> func)
        {
            return WatchingMethod(func.Method);
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}"/></param>
        /// <returns>
        /// Returns an instance of the class with type <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WatchingMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> func)
        {
            return WatchingMethod(func.Method);
        }

        #endregion

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
        /// Get custom data from performance meter information.
        /// </summary>
        /// <typeparam name="TResult">Type of custom data result.</typeparam>
        /// <param name="key">Key.</param>
        /// <returns>
        /// Returns typed result.
        /// </returns>
        public TResult GetCustomData<TResult>(string key)
        {
            if (this.CustomData.ContainsKey(key))
            {
                if (this.CustomData.TryRemove(key, out var result) && result is TResult typedResult)
                {
                    this.CustomData.TryAdd(key, typedResult);
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
        /// Get exception handler.
        /// </summary>
        /// <returns>
        /// Returns exception handler.
        /// </returns>
        public Action<Exception> GetExceptionHandler()
        {
            return this.ExceptionHandler;
        }

        /// <summary>
        /// Get default exception handler.
        /// </summary>
        /// <returns>
        /// Returns default exception handler.
        /// </returns>
        public static Action<Exception> GetDefaultExceptionHandler()
        {
            return DefaultExceptionHandler;
        }

        /// <summary>
        /// Attempts to add the specified key and value to the custom data.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <returns>
        /// Returns true if custom data was added.
        /// </returns>
        public bool TryAddCustomData(string key, object value)
        {
            return this.CustomData.TryAdd(key, value);
        }

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
            text.AppendLine();
            text.AppendLine($"Performance information for class : \"{performanceInfo.ClassName}\"");
            text.AppendLine();
            text.AppendLine("Current activity:");
            text.AppendLine();
            text.AppendFormat("| {0, 6} | {1}{2}", "Calls", "Method name", Environment.NewLine);
            text.AppendLine("|--------|------------------------------");
            foreach (var currentActivity in performanceInfo.CurrentActivity)
                text.AppendFormat("| {0, 6} | {1}{2}", currentActivity.CallsCount, currentActivity.MethodName, Environment.NewLine);

            text.AppendLine();
            text.AppendLine("Total activity:");
            text.AppendLine();
            text.AppendFormat("| {0, 6} | {1}{2}", "Calls", "Method name", Environment.NewLine);
            text.AppendLine("|--------|------------------------------");
            foreach (var totalActivity in performanceInfo.TotalActivity)
                text.AppendFormat("| {0, 6} | {1}{2}", totalActivity.CallsCount, totalActivity.MethodName, Environment.NewLine);

            text.AppendLine();
            text.AppendLine("Method calls:");
            text.AppendLine();

            text.AppendFormat("| {0, 20} | {1, 26} | {2, 20} | {3}{4}", "Elapsed", "Start time", "Caller", "Method name", Environment.NewLine);
            text.AppendLine("|----------------------|----------------------------|----------------------|------------------------------");
            foreach (var methodCalls in performanceInfo.MethodCalls.OrderBy(mc => mc.StartTime))
                text.AppendFormat("| {0, 20} | {1, 26} | {2, 20} | {3}{4}", methodCalls.Elapsed, $"{methodCalls.StartTime:yyyy-MM-dd HH:mm:ss.ffffff}", methodCalls.Caller, methodCalls.MethodName, Environment.NewLine);

            return text.ToString();
        }

        #region StartWatching

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

        #region With Action

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching(Action action)
        {
            return WatchingMethod(action.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T>(Action<T> action)
        {
            return WatchingMethod(action.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2>(Action<T1, T2> action)
        {
            return WatchingMethod(action.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3>(Action<T1, T2, T3> action)
        {
            return WatchingMethod(action.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action)
        {
            return WatchingMethod(action.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action)
        {
            return WatchingMethod(action.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action)
        {
            return WatchingMethod(action.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6, T7}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action)
        {
            return WatchingMethod(action.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6, T7, T8}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action)
        {
            return WatchingMethod(action.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6, T7, T8, T9}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action)
        {
            return WatchingMethod(action.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action)
        {
            return WatchingMethod(action.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action)
        {
            return WatchingMethod(action.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action)
        {
            return WatchingMethod(action.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action)
        {
            return WatchingMethod(action.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action)
        {
            return WatchingMethod(action.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action)
        {
            return WatchingMethod(action.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="action"><see cref="Action{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action)
        {
            return WatchingMethod(action.Method).Start();
        }

        #endregion

        #region With Func

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{TResult}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<TResult>(Func<TResult> func)
        {
            return WatchingMethod(func.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T, TResult}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T, TResult>(Func<T, TResult> func)
        {
            return WatchingMethod(func.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, TResult}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, TResult>(Func<T1, T2, TResult> func)
        {
            return WatchingMethod(func.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, TResult}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func)
        {
            return WatchingMethod(func.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, TResult}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func)
        {
            return WatchingMethod(func.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, TResult}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func)
        {
            return WatchingMethod(func.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, TResult}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func)
        {
            return WatchingMethod(func.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, T7, TResult}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> func)
        {
            return WatchingMethod(func.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, TResult}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func)
        {
            return WatchingMethod(func.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> func)
        {
            return WatchingMethod(func.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> func)
        {
            return WatchingMethod(func.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> func)
        {
            return WatchingMethod(func.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> func)
        {
            return WatchingMethod(func.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> func)
        {
            return WatchingMethod(func.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> func)
        {
            return WatchingMethod(func.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> func)
        {
            return WatchingMethod(func.Method).Start();
        }

        /// <summary>
        /// Create an instance of the class to watching method performance.
        /// </summary>
        /// <param name="func"><see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> StartWatching<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> func)
        {
            return WatchingMethod(func.Method).Start();
        }

        #endregion

        #endregion

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
        /// <param name="iterations">Number of executing Action iterations.</param>
        public void Inline(Action action, uint iterations = 1)
        {
            new CodeExecutorBuilder<TClass, Exception>(this).Start(action, iterations);
        }

        /// <summary>
        /// Execute the Task.
        /// </summary>
        /// <param name="task">Executed Task.</param>
        /// <param name="iterations">Number of executing Task iterations.</param>
        /// <returns>
        /// Returns <see cref="Task"/>.
        /// </returns>
        public async ValueTask InlineAsync(ValueTask task, uint iterations = 1)
        {
            await new CodeExecutorBuilder<TClass, Exception>(this).StartAsync(task, iterations);
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
        /// Returns Task of result.
        /// </returns>
        public async ValueTask<TResult> InlineAsync<TResult>(ValueTask<TResult> task, TResult defaultResult = default)
        {
            return await new CodeExecutorBuilder<TClass, Exception>(this).StartAsync(task, defaultResult);
        }

        #endregion

        #region InlineIgnored

        /// <summary>
        /// Execute the Action.
        /// </summary>
        /// <param name="action">Executed Action.</param>
        /// <param name="iterations">Number of executing Action iterations.</param>
        public void InlineIgnored(Action action, uint iterations = 1)
        {
            new CodeExecutorBuilder<TClass, Exception>(this).WithoutWatching().Start(action, iterations);
        }

        /// <summary>
        /// Execute the Task.
        /// </summary>
        /// <param name="task">Executed Task.</param>
        /// <param name="iterations">Number of executing Task iterations.</param>
        /// <returns>
        /// Returns <see cref="Task"/>.
        /// </returns>
        public async ValueTask InlineIgnoredAsync(ValueTask task, uint iterations = 1)
        {
            await new CodeExecutorBuilder<TClass, Exception>(this).WithoutWatching().StartAsync(task, iterations);
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
        public async ValueTask<TResult> InlineIgnoredAsync<TResult>(ValueTask<TResult> task, TResult defaultResult = default)
        {
            return await new CodeExecutorBuilder<TClass, Exception>(this).WithoutWatching().StartAsync(task, defaultResult);
        }

        #endregion

        #endregion
    }
}
