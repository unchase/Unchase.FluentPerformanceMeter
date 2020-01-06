using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DiagnosticAdapter;
using System.Linq;
using Unchase.FluentPerformanceMeter.AspNetCore.Mvc.Attributes;
using Unchase.FluentPerformanceMeter.Attributes;
using Unchase.FluentPerformanceMeter.Builders;

namespace Unchase.FluentPerformanceMeter.AspNetCore.Mvc.DiagnosticSource
{
    /// <summary>
    /// The class for watching performance with diagnostic source.
    /// </summary>
    /// <typeparam name="TClass">Class with public methods.</typeparam>
    public sealed class PerformanceClassDiagnosticObserver<TClass> : PerformanceDiagnosticObserverBase where TClass : class
    {
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
            var controllerActionDescriptor = (ControllerActionDescriptor)actionDescriptor;

            if (!controllerActionDescriptor.MethodInfo.GetCustomAttributes(typeof(WatchingWithDiagnosticSourceAttribute), false).Any()
                && !controllerActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(WatchingWithDiagnosticSourceAttribute), false).Any())
                return;

            if (controllerActionDescriptor.MethodInfo.GetCustomAttributes(typeof(IgnoreMethodPerformanceAttribute), false).Any())
                return;

            var performanceMeterBuilder = PerformanceMeter<TClass>
                .WatchingMethod(controllerActionDescriptor.ActionName)
                .WithSettingData;

            // add custom data from attributes
            foreach (MethodCustomDataAttribute methodCustomData in controllerActionDescriptor.MethodInfo.GetCustomAttributes(typeof(MethodCustomDataAttribute), false))
            {
                performanceMeterBuilder = performanceMeterBuilder.CustomData(methodCustomData.Key, methodCustomData.Value);
            }
            if (httpContext.Request.QueryString.HasValue)
                performanceMeterBuilder = performanceMeterBuilder.CustomData("queryString", httpContext.Request.QueryString.Value);
            if (httpContext.User.Identity.IsAuthenticated)
                performanceMeterBuilder = performanceMeterBuilder.CustomData("userIdentityName", httpContext.User.Identity.Name);

            // add caller from attributes
            performanceMeterBuilder = performanceMeterBuilder.CallerFrom(httpContext.Connection?.RemoteIpAddress?.ToString() ?? httpContext.Connection?.LocalIpAddress?.ToString());
            foreach (MethodCallerAttribute methodCaller in controllerActionDescriptor.MethodInfo.GetCustomAttributes(typeof(MethodCallerAttribute), false))
            {
                performanceMeterBuilder = performanceMeterBuilder.CallerFrom(methodCaller.Caller);
            }

            httpContext.Items["PerformanceMeter"] = performanceMeterBuilder.Start();
        }

        /// <summary>
        /// On Microsoft.AspNetCore.Hosting.HttpRequestIn.Start.
        /// </summary>
        /// <param name="httpContext"></param>
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
            if (!httpContext.Items.TryGetValue("PerformanceMeter", out object performanceMeter))
                return;

            ((PerformanceMeter<TClass>)performanceMeter).Dispose();
        }

        #endregion
    }
}
