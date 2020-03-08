using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DiagnosticAdapter;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Unchase.FluentPerformanceMeter.AspNetCore.Mvc.Attributes;
using Unchase.FluentPerformanceMeter.AspNetCore.Mvc.Common;
using Unchase.FluentPerformanceMeter.Builders;

namespace Unchase.FluentPerformanceMeter.AspNetCore.Mvc.DiagnosticSource
{
    /// <summary>
    /// The class for watching performance with diagnostic source.
    /// </summary>
    /// <typeparam name="TClass">Class with public methods.</typeparam>
    public sealed class PerformanceDiagnosticObserver<TClass> : PerformanceDiagnosticObserverBase where TClass : ControllerBase
    {
        #region Fields

        private readonly IOptions<PerformanceMeterMvcOptions<TClass>> _options;

        internal PerformanceMeterMvcOptions<TClass> Options => _options.Value;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for <see cref="PerformanceDiagnosticObserver{PerformanceMeterOptions}"/>.
        /// </summary>
        /// <param name="options">The options, containing the rules to apply.</param>
        public PerformanceDiagnosticObserver(IOptions<PerformanceMeterMvcOptions<TClass>> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Check matching the name.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>
        /// Returns true if name is match.
        /// </returns>
        protected override bool IsMatch(string name)
        {
            return name == "Microsoft.AspNetCore";
        }

        /// <summary>
        /// On Microsoft.AspNetCore.Hosting.HttpRequestIn.
        /// </summary>
        [DiagnosticName("Microsoft.AspNetCore.Hosting.HttpRequestIn")]
        public void OnHttpRequestIn() { }

        /// <summary>
        /// On Microsoft.AspNetCore.Mvc.BeforeAction.
        /// </summary>
        /// <param name="httpContext"><see cref="HttpContext"/>.</param>
        /// <param name="actionDescriptor"><see cref="ActionDescriptor"/>.</param>
        [DiagnosticName("Microsoft.AspNetCore.Mvc.BeforeAction")]
        public void OnBeforeAction(HttpContext httpContext, ActionDescriptor actionDescriptor)
        {
            if (PerformanceMeterCommonMethods.ShouldWatching(httpContext.Request, Options))
            {
                if (actionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                {
                    if (PerformanceMeterCommonMethods.CheckExcludedMethods(controllerActionDescriptor, Options))
                    {
                        return;
                    }

                    if (PerformanceMeterCommonMethods.CheckAnnotatedAttribute(controllerActionDescriptor, Options,
                        typeof(WatchingWithDiagnosticSourceAttribute)))
                    {
                        return;
                    }

                    if (PerformanceMeterCommonMethods.CheckIgnoreMethodPerformanceAttribute(controllerActionDescriptor,
                        Options))
                    {
                        return;
                    }

                    var performanceMeterBuilder = PerformanceMeter<TClass>
                        .WatchingMethod(controllerActionDescriptor.ActionName)
                        .WithSettingData;

                    // add custom data from custom attributes
                    performanceMeterBuilder =
                        performanceMeterBuilder.AddCustomDataFromCustomAttributes(httpContext, controllerActionDescriptor,
                            Options);

                    httpContext.Items["PerformanceMeter"] = performanceMeterBuilder.Start();
                }
            }
        }

        /// <summary>
        /// On Microsoft.AspNetCore.Hosting.HttpRequestIn.Start.
        /// </summary>
        /// <param name="httpContext"><see cref="HttpContext"/>.</param>
        [DiagnosticName("Microsoft.AspNetCore.Hosting.HttpRequestIn.Start")]
        public void OnHttpRequestInStart(HttpContext httpContext) { }

        /// <summary>
        /// On Microsoft.AspNetCore.Mvc.AfterAction.
        /// </summary>
        /// <param name="httpContext"><see cref="HttpContext"/>.</param>
        /// <param name="actionDescriptor"><see cref="ActionDescriptor"/>.</param>
        [DiagnosticName("Microsoft.AspNetCore.Mvc.AfterAction")]
        public void OnAfterAction(HttpContext httpContext, ActionDescriptor actionDescriptor) { }

        /// <summary>
        /// On Microsoft.AspNetCore.Hosting.HttpRequestIn.Stop.
        /// </summary>
        /// <param name="httpContext"><see cref="HttpContext"/>.</param>
        [DiagnosticName("Microsoft.AspNetCore.Hosting.HttpRequestIn.Stop")]
        public void OnHttpRequestInStop(HttpContext httpContext)
        {
            if (!httpContext.Items.TryGetValue("PerformanceMeter", out var performanceMeter))
                return;

            ((PerformanceMeter<TClass>)performanceMeter).Dispose();
        }

        /// <summary>
        /// Handle the Exception.
        /// </summary>
        /// <param name="error">Exception.</param>
        protected override void HandleException(Exception error)
        {
            var exceptionHandler = PerformanceMeter<TClass>.GetDefaultExceptionHandler();
            exceptionHandler(error);
        }

        #endregion
    }
}
