using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Unchase.FluentPerformanceMeter.Attributes;
using Unchase.FluentPerformanceMeter.Builders;
using Unchase.FluentPerformanceMeter.Extensions;

namespace Unchase.FluentPerformanceMeter.AspNetCore.Mvc.Common
{
    internal static class PerformanceMeterCommonMethods
    {
        #region Properties

        internal static string QueryStringCustomDataKey { get; set; } = "pm_queryString";

        internal static string UserIdentityNameCustomDataKey { get; } = "pm_userIdentityName";

        internal static string PathCustomDataKey { get; } = "pm_path";

        #endregion

        #region Methods

        internal static bool ShouldWatching<TClass>(HttpRequest request, PerformanceMeterMvcOptions<TClass> options)
            where TClass : ControllerBase
        {
            foreach (var ignored in options.IgnoredPaths)
            {
                if (ignored != null && request.Path.Value.Contains(ignored, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return options.ShouldWatching?.Invoke(request) ?? true;
        }

        internal static bool CheckExcludedMethods<TClass>(ControllerActionDescriptor controllerActionDescriptor,
            PerformanceMeterMvcOptions<TClass> options) where TClass : ControllerBase
        {
            return options?.ExcludedMethods?.Contains(controllerActionDescriptor.ActionName) ?? false;
        }

        internal static bool CheckAnnotatedAttribute<TClass>(ControllerActionDescriptor controllerActionDescriptor,
            PerformanceMeterMvcOptions<TClass> options, Type attributeType) where TClass : ControllerBase
        {
            return (options == null || options.WatchForAnnotatedWithAttributeOnly)
                   && !controllerActionDescriptor.MethodInfo
                       .GetCustomAttributes(attributeType, false).Any()
                   && !controllerActionDescriptor.ControllerTypeInfo
                       .GetCustomAttributes(attributeType, false).Any();
        }

        internal static bool CheckIgnoreMethodPerformanceAttribute<TClass>(
            ControllerActionDescriptor controllerActionDescriptor, PerformanceMeterMvcOptions<TClass> options)
            where TClass : ControllerBase
        {
            return (options == null || options.UseIgnoreMethodPerformanceAttribute)
                   && controllerActionDescriptor.MethodInfo
                       .GetCustomAttributes(typeof(IgnoreMethodPerformanceAttribute), false).Any();
        }

        internal static SettingsBuilder<TClass> AddCustomDataFromCustomAttributes<TClass>(this SettingsBuilder<TClass> settingsBuilder, HttpContext context, ControllerActionDescriptor controllerActionDescriptor, PerformanceMeterMvcOptions<TClass> options) where TClass : ControllerBase
        {
            if (options == null || options.AddCustomDataFromCustomAttributes)
            {
                foreach (MethodCustomDataAttribute methodCustomData in controllerActionDescriptor.MethodInfo.GetCustomAttributes(typeof(MethodCustomDataAttribute), false))
                {
                    settingsBuilder = settingsBuilder.CustomData(methodCustomData.Key, methodCustomData.Value);
                }
                if (context.Request.QueryString.HasValue)
                    settingsBuilder = settingsBuilder.CustomData(QueryStringCustomDataKey, context.Request.QueryString.Value);
                if (context.User.Identity.IsAuthenticated)
                    settingsBuilder = settingsBuilder.CustomData(UserIdentityNameCustomDataKey, context.User.Identity.Name);

                // add caller from attributes
                settingsBuilder = settingsBuilder.CallerFrom(context.Connection?.RemoteIpAddress?.ToString() ?? context.Connection?.LocalIpAddress?.ToString());
                foreach (MethodCallerAttribute methodCaller in controllerActionDescriptor.MethodInfo.GetCustomAttributes(typeof(MethodCallerAttribute), false))
                {
                    settingsBuilder = settingsBuilder.CallerFrom(methodCaller.Caller);
                }
            }

            // add route path to custom data
            if (options == null || options.AddRoutePathToCustomData)
            {
                var url = StringBuilderCache.Get()
                    .Append(context.Request.Scheme)
                    .Append("://")
                    .Append(context.Request.Host.Value)
                    .Append(context.Request.PathBase.Value)
                    .Append(context.Request.Path.Value)
                    .Append(context.Request.QueryString.Value)
                    .ToStringRecycle();

                var routeData = context.GetRouteData();
                if (routeData != null)
                {
                    settingsBuilder = settingsBuilder.CustomData(PathCustomDataKey, routeData.Values["controller"] + "/" + routeData.Values["action"]);
                }
                else
                {
                    if (url.Length > 50)
                        url = url.Remove(50);
                    settingsBuilder = settingsBuilder.CustomData(PathCustomDataKey, url);
                }
            }

            return settingsBuilder;
        }

        #endregion
    }
}
