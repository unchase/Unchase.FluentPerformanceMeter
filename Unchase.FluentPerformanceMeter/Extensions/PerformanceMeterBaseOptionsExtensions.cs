namespace Unchase.FluentPerformanceMeter.Extensions
{
    /// <summary>
    /// Handy extensions for <see cref="PerformanceMeterBaseOptions"/>.
    /// </summary>
    public static class PerformanceMeterBaseOptionsExtensions
    {
        #region Extension methods

        /// <summary>
        /// Excludes a method from performance watching, convenience method for chaining, basically <see cref="PerformanceMeterBaseOptions.ExcludedMethods"/>.Add(name)
        /// </summary>
        /// <typeparam name="T">The subtype of <see cref="PerformanceMeterBaseOptions"/> to use (inferred for common usage).</typeparam>
        /// <param name="options">The options to exclude the method on.</param>
        /// <param name="method">The method name to exclude from performance watching.</param>
        public static T ExcludeMethod<T>(this T options, string method) where T : PerformanceMeterBaseOptions
        {
            options.ExcludedMethods.Add(method);
            return options;
        }

        #endregion
    }
}
