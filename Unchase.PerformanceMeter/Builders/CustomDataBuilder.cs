using System.Runtime.CompilerServices;

namespace Unchase.PerformanceMeter.Builders
{
    /// <summary>
    /// Class for setting custom data for <see cref="PerformanceMeter{TClass}"/>.
    /// </summary>
    /// <typeparam name="TClass">Class with methods.</typeparam>
    public sealed class CustomDataBuilder<TClass> : PerformanceMeterBuilder<TClass> where TClass : class
    {
        #region Constructors

        /// <summary>
        /// Constructor for <see cref="CustomDataBuilder{TClass}"/>.
        /// </summary>
        /// <param name="performanceMeter"><see cref="PerformanceMeter{TClass}"/>.</param>
        public CustomDataBuilder(PerformanceMeter<TClass> performanceMeter)
        {
            this.PerformanceMeter = performanceMeter;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add custom data.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <returns>
        /// Returns <see cref="CustomDataBuilder{TClass}"/>.
        /// </returns>
        internal CustomDataBuilder<TClass> AddCustomData(string key, object value)
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
        /// Returns <see cref="CustomDataBuilder{TClass}"/>.
        /// </returns>
        internal CustomDataBuilder<TClass> AddCallerData(string callerSource = "", int callerSourceLineNumber = 0)
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
    /// Extension methods for the <see cref="PerformanceMeterBuilder{TClass}"/>
    /// </summary>
    public static class CustomDataBuilderExtensions
    {
        #region Extension methods

        /// <summary>
        /// Add custom data.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="customDataBuilder"><see cref="CustomDataBuilder{TClass}"/>.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <returns>
        /// Returns <see cref="CustomDataBuilder{TClass}"/>.
        /// </returns>
        public static CustomDataBuilder<TClass> WithCustomData<TClass>(this CustomDataBuilder<TClass> customDataBuilder, string key, object value) where TClass : class
        {
            return customDataBuilder.AddCustomData(key, value);
        }

        /// <summary>
        /// Add caller data.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="customDataBuilder"><see cref="CustomDataBuilder{TClass}"/>.</param>
        /// <param name="callerSource">Caller source.</param>
        /// <param name="callerSourceLineNumber">Caller source line number.</param>
        /// <returns>
        /// Returns <see cref="CustomDataBuilder{TClass}"/>.
        /// </returns>
        public static CustomDataBuilder<TClass> WithCallerData<TClass>(this CustomDataBuilder<TClass> customDataBuilder,
            [CallerFilePath] string callerSource = "",
            [CallerLineNumber] int callerSourceLineNumber = 0) where TClass : class
        {
            return customDataBuilder.AddCallerData(callerSource, callerSourceLineNumber);
        }

        #endregion
    }
}
