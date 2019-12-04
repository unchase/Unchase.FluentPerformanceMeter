namespace Unchase.FluentPerformanceMeter.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="PerformanceMeter{TClass}"/>.
    /// </summary>
    public static class PerformanceMeterExtensions
    {
        #region Extension methods

        /// <summary>
        /// Add performance meter step.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="performanceMeter"><see cref="PerformanceMeter{TClass}"/>.</param>
        /// <param name="stepName">Step name.</param>
        /// <returns>
        /// Returns <see cref="PerformanceInfoStep{TClass}"/>.
        /// </returns>
        public static PerformanceInfoStep<TClass> Step<TClass>(this PerformanceMeter<TClass> performanceMeter, string stepName) where TClass : class
        {
            return PerformanceInfoStep<TClass>.WatchingStep(performanceMeter, stepName);
        }

        /// <summary>
        /// Add performance meter step, but only saves when over <paramref name="minSaveMs"/>.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="performanceMeter"><see cref="PerformanceMeter{TClass}"/>.</param>
        /// <param name="stepName">Step name.</param>
        /// <param name="minSaveMs">The minimum time to take before this step is saved (e.g. if it’s fast, leave it out).</param>
        /// <returns>
        /// Returns <see cref="PerformanceInfoStep{TClass}"/>.
        /// </returns>
        public static PerformanceInfoStep<TClass> StepIf<TClass>(this PerformanceMeter<TClass> performanceMeter, string stepName, double minSaveMs) where TClass : class
        {
            return PerformanceInfoStep<TClass>.WatchingStepIf(performanceMeter, stepName, minSaveMs);
        }

        /// <summary>
        /// Silences a performance meter for the duration, use in a using to silence for the duration.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <returns>
        /// Returns <see cref="PerformanceInfoStep{TClass}"/>.
        /// </returns>
        public static PerformanceInfoStep<TClass> Ignore<TClass>(this PerformanceMeter<TClass> performanceMeter) where TClass : class
        {
            return PerformanceInfoStep<TClass>.WatchingStepIf(performanceMeter, string.Empty, double.MaxValue).WithoutWatching();
        }

        #endregion
    }
}
