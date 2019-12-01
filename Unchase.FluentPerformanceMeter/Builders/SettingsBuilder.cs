using Microsoft.AspNetCore.Http;
using System;
using System.Runtime.CompilerServices;
using Unchase.FluentPerformanceMeter.Attributes;

namespace Unchase.FluentPerformanceMeter.Builders
{
    /// <summary>
    /// Class for setting custom data for <see cref="PerformanceMeter{TClass}"/>.
    /// </summary>
    /// <typeparam name="TClass">Class with methods.</typeparam>
    public sealed class SettingsBuilder<TClass> : PerformanceMeterBuilder<TClass> where TClass : class
    {
        #region Constructors

        /// <summary>
        /// Constructor for <see cref="SettingsBuilder{TClass}"/>.
        /// </summary>
        /// <param name="performanceMeter"><see cref="PerformanceMeter{TClass}"/>.</param>
        public SettingsBuilder(PerformanceMeter<TClass> performanceMeter)
        {
            this.PerformanceMeter = performanceMeter;
            foreach (var performanceCustomDataAttribute in performanceMeter.MethodInfo.GetCustomAttributes(typeof(MethodCustomDataAttribute), false))
            {
                if (performanceCustomDataAttribute is MethodCustomDataAttribute customDataAttribute)
                    AddCustomData(customDataAttribute.Key, customDataAttribute.Value);
            }

            foreach (var methodCallerAttribute in performanceMeter.MethodInfo.GetCustomAttributes(typeof(MethodCallerAttribute), false))
            {
                if (methodCallerAttribute is MethodCallerAttribute caller)
                    WithCaller(caller.Caller);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set <see cref="IHttpContextAccessor"/> to get the ip address of the caller.
        /// </summary>
        /// <param name="httpContextAccessor"><see cref="IHttpContextAccessor"/>.</param>
        /// <returns>
        /// Returns <see cref="SettingsBuilder{TClass}"/>.
        /// </returns>
        internal SettingsBuilder<TClass> WithHttpContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            this.PerformanceMeter.HttpContextAccessor = httpContextAccessor;
            return this;
        }

        /// <summary>
        /// Set Action to handle exceptions that occur.
        /// </summary>
        /// <param name="exceptionHandler">Action to handle exceptions that occur.</param>
        /// <returns>
        /// Returns <see cref="SettingsBuilder{TClass}"/>.
        /// </returns>
        internal SettingsBuilder<TClass> WithExceptionHandler(Action<Exception> exceptionHandler = null)
        {
            this.PerformanceMeter.ExceptionHandler = exceptionHandler;
            return this;
        }

        /// <summary>
        /// Set caller name.
        /// </summary>
        /// <param name="caller">Caller name.</param>
        /// <returns>
        /// Returns <see cref="SettingsBuilder{TClass}"/>.
        /// </returns>
        internal SettingsBuilder<TClass> WithCaller(string caller)
        {
            this.PerformanceMeter.Caller = caller;
            return this;
        }

        /// <summary>
        /// Add custom data.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <returns>
        /// Returns <see cref="SettingsBuilder{TClass}"/>.
        /// </returns>
        internal SettingsBuilder<TClass> AddCustomData(string key, object value)
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
        /// Returns <see cref="SettingsBuilder{TClass}"/>.
        /// </returns>
        internal SettingsBuilder<TClass> AddCallerSourceData(string callerSource = "", int callerSourceLineNumber = 0)
        {
            if (!string.IsNullOrWhiteSpace(callerSource))
                this.AddCustomData(nameof(callerSource), callerSource);

            if (callerSourceLineNumber > 0)
                this.AddCustomData(nameof(callerSourceLineNumber), callerSourceLineNumber);

            return this;
        }

        #endregion
    }

    /// <summary>
    /// Extension methods for the <see cref="SettingsBuilder{TClass}"/>
    /// </summary>
    public static class SettingsBuilderExtensions
    {
        #region Extension methods

        /// <summary>
        /// Set <see cref="IHttpContextAccessor"/> to get the ip address of the caller.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="settingsBuilder"><see cref="SettingsBuilder{TClass}"/>.</param>
        /// <param name="httpContextAccessor"><see cref="IHttpContextAccessor"/>.</param>
        /// <returns>
        /// Returns <see cref="SettingsBuilder{TClass}"/>.
        /// </returns>
        public static SettingsBuilder<TClass> CallerFrom<TClass>(this SettingsBuilder<TClass> settingsBuilder, IHttpContextAccessor httpContextAccessor) where TClass : class
        {
            return settingsBuilder.WithHttpContextAccessor(httpContextAccessor);
        }

        /// <summary>
        /// Set Action to handle exceptions that occur.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="settingsBuilder"><see cref="SettingsBuilder{TClass}"/>.</param>
        /// <param name="exceptionHandler">Action to handle exceptions that occur.</param>
        /// <returns>
        /// Returns <see cref="SettingsBuilder{TClass}"/>.
        /// </returns>
        public static SettingsBuilder<TClass> ExceptionHandler<TClass>(this SettingsBuilder<TClass> settingsBuilder, Action<Exception> exceptionHandler = null) where TClass : class
        {
            return settingsBuilder.WithExceptionHandler(exceptionHandler);
        }

        /// <summary>
        /// Set caller name.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="settingsBuilder"><see cref="SettingsBuilder{TClass}"/>.</param>
        /// <param name="caller">Caller name.</param>
        /// <returns>
        /// Returns <see cref="SettingsBuilder{TClass}"/>.
        /// </returns>
        public static SettingsBuilder<TClass> CallerFrom<TClass>(this SettingsBuilder<TClass> settingsBuilder, string caller) where TClass : class
        {
            return settingsBuilder.WithCaller(caller);
        }

        /// <summary>
        /// Add custom data.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="settingsBuilder"><see cref="SettingsBuilder{TClass}"/>.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <returns>
        /// Returns <see cref="SettingsBuilder{TClass}"/>.
        /// </returns>
        public static SettingsBuilder<TClass> CustomData<TClass>(this SettingsBuilder<TClass> settingsBuilder, string key, object value) where TClass : class
        {
            return settingsBuilder.AddCustomData(key, value);
        }

        /// <summary>
        /// Add caller data.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="settingsBuilder"><see cref="SettingsBuilder{TClass}"/>.</param>
        /// <param name="callerSource">Caller source.</param>
        /// <param name="callerSourceLineNumber">Caller source line number.</param>
        /// <returns>
        /// Returns <see cref="SettingsBuilder{TClass}"/>.
        /// </returns>
        public static SettingsBuilder<TClass> CallerSourceData<TClass>(this SettingsBuilder<TClass> settingsBuilder,
            [CallerFilePath] string callerSource = "",
            [CallerLineNumber] int callerSourceLineNumber = 0) where TClass : class
        {
            return settingsBuilder.AddCallerSourceData(callerSource, callerSourceLineNumber);
        }

        #endregion
    }
}
