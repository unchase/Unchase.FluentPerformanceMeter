namespace Unchase.FluentPerformanceMeter.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="PerformanceInfoStep{TClass}"/>.
    /// </summary>
    public static class PerformanceInfoStepExtensions
    {
        #region Extension methods

        /// <summary>
        /// Add custom data to performance meter step information.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="performanceInfoStep"><see cref="PerformanceInfoStep{TClass}"/>.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <returns>
        /// Returns <see cref="PerformanceInfoStep{TClass}"/>.
        /// </returns>
        public static PerformanceInfoStep<TClass> AddCustomData<TClass>(this PerformanceInfoStep<TClass> performanceInfoStep, string key, object value) where TClass : class
        {
            return performanceInfoStep.AddCustomData(key, value);
        }

        /// <summary>
        /// Silences a performance meter for the duration, use in a using to silence for the duration.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="performanceInfoStep"><see cref="PerformanceInfoStep{TClass}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceInfoStep{TClass}"/>.
        /// </returns>
        public static PerformanceInfoStep<TClass> WithoutWatching<TClass>(this PerformanceInfoStep<TClass> performanceInfoStep) where TClass : class
        {
            return performanceInfoStep.WithoutWatching();
        }

        #endregion
    }
}
