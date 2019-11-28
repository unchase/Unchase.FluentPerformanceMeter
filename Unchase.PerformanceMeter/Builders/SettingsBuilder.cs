using Microsoft.AspNetCore.Http;
using System;

namespace Unchase.PerformanceMeter.Builders
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

        #endregion
    }
}
