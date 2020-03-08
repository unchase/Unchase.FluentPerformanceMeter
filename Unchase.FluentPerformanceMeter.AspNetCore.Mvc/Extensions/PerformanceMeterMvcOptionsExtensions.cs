using System;
using Microsoft.AspNetCore.Mvc;

namespace Unchase.FluentPerformanceMeter.AspNetCore.Mvc.Extensions
{
    /// <summary>
    /// Handy extensions for <see cref="PerformanceMeterMvcOptions{TClass}"/>.
    /// </summary>
    public static class PerformanceMeterMvcOptionsExtensions
    {
        #region Extension methods

        /// <summary>
        /// Excludes a path from being watched, convenience method for chaining, basically <see cref="PerformanceMeterMvcOptions{TClass}.IgnoredPaths"/>.Add(assembly)
        /// </summary>
        /// <typeparam name="TClass">Type of the controller.</typeparam>
        /// <param name="options">The options to exclude the type on.</param>
        /// <param name="path">The path to exclude from profiled.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeterMvcOptions{TClass}"/>.
        /// </returns>
        public static PerformanceMeterMvcOptions<TClass> IgnorePath<TClass>(this PerformanceMeterMvcOptions<TClass> options, string path) where TClass : ControllerBase
        {
            options.IgnoredPaths.Add(path);
            return options;
        }

        /// <summary>
        /// Add common custom data of the controller.
        /// </summary>
        /// <typeparam name="TClass">Type of the controller.</typeparam>
        /// <param name="options">The options.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeterMvcOptions{TClass}"/>.
        /// </returns>
        public static PerformanceMeterMvcOptions<TClass> AddCustomData<TClass>(this PerformanceMeterMvcOptions<TClass> options, string key, object value) where TClass : ControllerBase
        {
            PerformanceMeter<TClass>.AddCustomData(key, value);
            return options;
        }

        /// <summary>
        /// Set the time in minutes to clear collection of the method calls.
        /// </summary>
        /// <typeparam name="TClass">Type of the controller.</typeparam>
        /// <param name="options">The options.</param>
        /// <param name="minutes">Time in minutes to clear list of the method calls.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeterMvcOptions{TClass}"/>.
        /// </returns>
        public static PerformanceMeterMvcOptions<TClass> SetMethodCallsCacheTime<TClass>(this PerformanceMeterMvcOptions<TClass> options, int minutes) where TClass : ControllerBase
        {
            PerformanceMeter<TClass>.SetMethodCallsCacheTime(minutes);
            return options;
        }

        /// <summary>
        /// Set Action to handle exceptions that occur by default.
        /// </summary>
        /// <typeparam name="TClass">Type of the controller.</typeparam>
        /// <param name="options">The options.</param>
        /// <param name="exceptionHandler">Action to handle exceptions that occur.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeterMvcOptions{TClass}"/>.
        /// </returns>
        public static PerformanceMeterMvcOptions<TClass> SetDefaultExceptionHandler<TClass>(this PerformanceMeterMvcOptions<TClass> options, Action<Exception> exceptionHandler) where TClass : ControllerBase
        {
            PerformanceMeter<TClass>.SetDefaultExceptionHandler(exceptionHandler);
            return options;
        }

        #endregion
    }
}
