using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Unchase.FluentPerformanceMeter.AspNetCore.Mvc
{
    /// <summary>
    /// Options for configuring Unchase.FluentPerformanceMeter.
    /// </summary>
    public class PerformanceMeterMvcOptions<TClass> : PerformanceMeterBaseOptions where TClass : ControllerBase
    {
        #region Properties

        /// <summary>
        /// When <see cref="PerformanceMeter{TClass}.StartWatching(string)"/> is called, if the current request URL contains any items in this property,
        /// no performance watching will be instantiated and no results will be displayed.
        /// Default value is { "/content/", "/scripts/", "/favicon.ico" }.
        /// </summary>
        public HashSet<string> IgnoredPaths { get; } = new HashSet<string> {
            "/content/",
            "/scripts/",
            "/favicon.ico"
        };

        /// <summary>
        /// Set a function to control whether a given request should be watching at all.
        /// </summary>
        public Func<HttpRequest, bool> ShouldWatching { get; set; } = _ => true;

        /// <summary>
        /// Watch actions performance annotated with special attribute.
        /// Default value is true.
        /// </summary>
        public bool WatchForAnnotatedWithAttributeOnly { get; set; } = true;

        /// <summary>
        /// Add route path to custom data.
        /// Default value is true.
        /// </summary>
        public bool AddRoutePathToCustomData { get; set; } = true;

        #endregion
    }
}
