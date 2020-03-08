using System;

namespace Unchase.FluentPerformanceMeter.Extensions
{
    /// <summary>
    /// Common extension methods to use only in this project
    /// </summary>
    public static class CommonExtensions
    {
        #region Extension methods

        /// <summary>
        /// Checks if a string contains another one.
        /// </summary>
        /// <param name="s">The string to check for presence in.</param>
        /// <param name="value">The value to check presence of.</param>
        /// <param name="comparison">The <see cref="StringComparison"/> to use when comparing.</param>
        /// <returns>Whether <paramref name="value"/> is contained in <paramref name="s"/>.</returns>
        public static bool Contains(this string s, string value, StringComparison comparison) =>
            s.IndexOf(value, comparison) >= 0;

        #endregion
    }
}
