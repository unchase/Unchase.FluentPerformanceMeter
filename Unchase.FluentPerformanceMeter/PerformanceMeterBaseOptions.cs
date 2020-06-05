using System;
using System.Collections.Generic;
using System.Reflection;
using Unchase.FluentPerformanceMeter.Attributes;

namespace Unchase.FluentPerformanceMeter
{
    /// <summary>
    /// Various configuration properties for Unchase.FluentPerformanceMeter.
    /// </summary>
    public class PerformanceMeterBaseOptions
    {
        #region Properties

        /// <summary>
        /// Assembly version of the Unchase.FluentPerformanceMeter.
        /// </summary>
        public static Version Version { get; } = typeof(PerformanceMeterBaseOptions).GetTypeInfo().Assembly.GetName().Version;

        /// <summary>
        /// The hash to use for file cache breaking, this is automatically calculated.
        /// </summary>
        public virtual string VersionHash { get; } = typeof(PerformanceMeterBaseOptions).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? Version.ToString();

        /// <summary>
        /// Methods to exclude from performance watching.
        /// </summary>
        public HashSet<string> ExcludedMethods { get; } = new HashSet<string>
        {
            "lambda_method",
            ".ctor"
        };

        /// <summary>
        /// Allow to use <see cref="IgnoreMethodPerformanceAttribute"/> for excluding from performance watching.
        /// Default value is true.
        /// </summary>
        public bool UseIgnoreMethodPerformanceAttribute { get; set; } = true;

        /// <summary>
        /// Add custom data from custom attributes.
        /// Default value is true.
        /// </summary>
        public bool AddCustomDataFromCustomAttributes { get; set; } = true;

        /// <summary>
        /// Adds a scope service of the PerformanceMeter of Class.
        /// Default value is true.
        /// </summary>
        public bool RegisterPerformanceMeterScope { get; set; } = true;

        #endregion

        #region Methods

        /// <summary>
        /// Called when passed to <see cref="PerformanceMeter{TClass}.Configure{T}(T)"/>.
        /// </summary>
        protected virtual void OnConfigure() { }

        internal void Configure() => OnConfigure();

        #endregion
    }
}
