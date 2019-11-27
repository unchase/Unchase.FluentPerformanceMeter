using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Unchase.PerformanceMeter
{
    /// <summary>
    /// Class for create and configure <see cref="PerformanceMeter{TClass}"/>.
    /// </summary>
    /// <typeparam name="TClass">Class with methods.</typeparam>
    public class PerformanceMeterBuilder<TClass> where TClass : class
    {
        #region Fields

        internal PerformanceMeter<TClass> PerformanceMeter;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </summary>
        /// <param name="performanceMeter"><see cref="PerformanceMeter{TClass}"/>.</param>
        public PerformanceMeterBuilder(PerformanceMeter<TClass> performanceMeter)
        {
            this.PerformanceMeter = performanceMeter;
        }

        #endregion

        #region Main

        /// <summary>
        /// Start watching method performance.
        /// </summary>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        internal PerformanceMeter<TClass> Start()
        {
            try
            {
                this.PerformanceMeter.DateStart = DateTime.Now;
                this.PerformanceMeter.Sw = Stopwatch.StartNew();
                Performance<TClass>.Input(this.PerformanceMeter.MethodInfo);
            }
            catch (Exception ex)
            {
                if (this.PerformanceMeter.ExceptionHandler != null)
                    this.PerformanceMeter.ExceptionHandler(ex);
                else
                    throw ex;
            }
            return this.PerformanceMeter;
        }

        #endregion

        #region Additional

        /// <summary>
        /// Set <see cref="IHttpContextAccessor"/> to get the ip address of the caller.
        /// </summary>
        /// <param name="httpContextAccessor"><see cref="IHttpContextAccessor"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        internal PerformanceMeterBuilder<TClass> WithHttpContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            this.PerformanceMeter.HttpContextAccessor = httpContextAccessor;
            return this;
        }

        /// <summary>
        /// Set Action to handle exceptions that occur.
        /// </summary>
        /// <param name="exceptionHandler">Action to handle exceptions that occur.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        internal PerformanceMeterBuilder<TClass> WithExceptionHandler(Action<Exception> exceptionHandler = null)
        {
            this.PerformanceMeter.ExceptionHandler = exceptionHandler;
            return this;
        }

        /// <summary>
        /// Set caller name.
        /// </summary>
        /// <param name="caller">Caller name.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        internal PerformanceMeterBuilder<TClass> WithCaller(string caller)
        {
            this.PerformanceMeter.Caller = caller;
            return this;
        }

        /// <summary>
        /// Register commands which will be executed after the performance watching is completed.
        /// </summary>
        /// <param name="performanceCommands">Collection of the executed commands.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        internal PerformanceMeterBuilder<TClass> WithExecutingOnComplete(params IPerformanceCommand[] performanceCommands)
        {
            foreach (var performanceCommand in performanceCommands)
            {
                this.PerformanceMeter.RegisteredCommands.Add(performanceCommand);
            }
            return this;
        }

        /// <summary>
        /// Add custom data.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        internal PerformanceMeterBuilder<TClass> WithCustomData(string key, object value)
        {
            this.PerformanceMeter.CustomData.TryAdd(key, value);
            return this;
        }

        /// <summary>
        /// Add caller data.
        /// </summary>
        /// <param name="callerSource">Caller source.</param>
        /// <param name="callerSourceLineNumber">Caller source line number.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        internal PerformanceMeterBuilder<TClass> WithCallerData(string callerSource = "", int callerSourceLineNumber = 0)
        {
            if (!string.IsNullOrWhiteSpace(callerSource))
                this.WithCustomData(nameof(callerSource), callerSource);

            if (callerSourceLineNumber > 0)
                this.WithCustomData(nameof(callerSourceLineNumber), callerSourceLineNumber);

            return this;
        }

        #endregion
    }

    /// <summary>
    /// Extension methods for the <see cref="PerformanceMeterBuilder{TClass}"/>
    /// </summary>
    public static class PPerformanceMeterBuilderExtensions
    {
        #region Extension methods

        /// <summary>
        /// Set <see cref="IHttpContextAccessor"/> to get the ip address of the caller.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="performanceMeterBuilder"><see cref="PerformanceMeterBuilder{TClass}"/>.</param>
        /// <param name="httpContextAccessor"><see cref="IHttpContextAccessor"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WithHttpContextAccessor<TClass>(this PerformanceMeterBuilder<TClass> performanceMeterBuilder, IHttpContextAccessor httpContextAccessor) where TClass : class
        {
            return performanceMeterBuilder.WithHttpContextAccessor(httpContextAccessor);
        }

        /// <summary>
        /// Set Action to handle exceptions that occur.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="performanceMeterBuilder"><see cref="PerformanceMeterBuilder{TClass}"/>.</param>
        /// <param name="exceptionHandler">Action to handle exceptions that occur.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WithExceptionHandler<TClass>(this PerformanceMeterBuilder<TClass> performanceMeterBuilder, Action<Exception> exceptionHandler = null) where TClass : class
        {
            return performanceMeterBuilder.WithExceptionHandler(exceptionHandler);
        }

        /// <summary>
        /// Register commands which will be executed after the performance watching is completed.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="performanceMeterBuilder"><see cref="PerformanceMeterBuilder{TClass}"/>.</param>
        /// <param name="performanceCommands">Collection of the executed commands.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WithExecutingOnComplete<TClass>(this PerformanceMeterBuilder<TClass> performanceMeterBuilder, params IPerformanceCommand[] performanceCommands) where TClass : class
        {
            return performanceMeterBuilder.WithExecutingOnComplete(performanceCommands);
        }

        /// <summary>
        /// Set caller name.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="performanceMeterBuilder"><see cref="PerformanceMeterBuilder{TClass}"/>.</param>
        /// <param name="caller">Caller name.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WithCaller<TClass>(this PerformanceMeterBuilder<TClass> performanceMeterBuilder, string caller) where TClass : class
        {
            return performanceMeterBuilder.WithCaller(caller);
        }

        /// <summary>
        /// Add custom data.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="performanceMeterBuilder"><see cref="PerformanceMeterBuilder{TClass}"/>.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WithCustomData<TClass>(this PerformanceMeterBuilder<TClass> performanceMeterBuilder, string key, object value) where TClass : class
        {
            return performanceMeterBuilder.WithCustomData(key, value);
        }

        /// <summary>
        /// Add caller data.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="performanceMeterBuilder"></param>
        /// <param name="callerSource">Caller source.</param>
        /// <param name="callerSourceLineNumber">Caller source line number.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </returns>
        public static PerformanceMeterBuilder<TClass> WithCallerData<TClass>(this PerformanceMeterBuilder<TClass> performanceMeterBuilder,
            [CallerFilePath] string callerSource = "",
            [CallerLineNumber] int callerSourceLineNumber = 0) where TClass : class
        {
            return performanceMeterBuilder.WithCallerData(callerSource, callerSourceLineNumber);
        }

        /// <summary>
        /// Start watching method performance.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="performanceMeterBuilder"><see cref="PerformanceMeterBuilder{TClass}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> Start<TClass>(this PerformanceMeterBuilder<TClass> performanceMeterBuilder) where TClass : class
        {
            return performanceMeterBuilder.Start();
        }

        #endregion
    }
}
