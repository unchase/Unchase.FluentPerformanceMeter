using System;
using System.Diagnostics;

namespace Unchase.FluentPerformanceMeter.Builders
{
    /// <summary>
    /// Class for create and configure <see cref="PerformanceMeter{TClass}"/>.
    /// </summary>
    /// <typeparam name="TClass">Class with methods.</typeparam>
    public class PerformanceMeterBuilder<TClass> where TClass : class
    {
        #region Properties and fields

        internal PerformanceMeter<TClass> PerformanceMeter;

        /// <summary>
        /// Allows to register commands which will be executed after the performance watching is completed.
        /// </summary>
        public ExecutedCommandsBuilder<TClass> WithExecutingOnComplete => new ExecutedCommandsBuilder<TClass>(this.PerformanceMeter);

        /// <summary>
        /// Allows to configure method performance watching settings.
        /// </summary>
        public SettingsBuilder<TClass> WithSetting => new SettingsBuilder<TClass>(this.PerformanceMeter);

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for <see cref="PerformanceMeterBuilder{TCalss}"/>.
        /// </summary>
        internal PerformanceMeterBuilder() { }

        /// <summary>
        /// Constructor for <see cref="PerformanceMeterBuilder{TClass}"/>.
        /// </summary>
        /// <param name="performanceMeter"><see cref="PerformanceMeter{TClass}"/>.</param>
        public PerformanceMeterBuilder(PerformanceMeter<TClass> performanceMeter)
        {
            this.PerformanceMeter = performanceMeter;
        }

        #endregion

        #region Main

        /// <summary>
        /// Start watching method performance.
        /// </summary>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        internal PerformanceMeter<TClass> Start()
        {
            try
            {
                Performance<TClass>.Input(this.PerformanceMeter.MethodInfo);
                this.PerformanceMeter.InnerStopwatch = Stopwatch.StartNew();
                this.PerformanceMeter.DateStart = DateTime.UtcNow - this.PerformanceMeter.InnerStopwatch.Elapsed;
            }
            catch (Exception ex)
            {
                if (this.PerformanceMeter.ExceptionHandler != null)
                    this.PerformanceMeter.ExceptionHandler(ex);
                else
                    throw;
            }
            return this.PerformanceMeter;
        }

        #endregion
    }

    /// <summary>
    /// Extension methods for the <see cref="PerformanceMeterBuilder{TClass}"/>
    /// </summary>
    public static class PerformanceMeterBuilderExtensions
    {
        #region Extension methods

        /// <summary>
        /// Start watching method performance.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="performanceMeterBuilder"><see cref="PerformanceMeterBuilder{TClass}"/>.</param>
        /// <returns>
        /// Returns <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> Start<TClass>(this PerformanceMeterBuilder<TClass> performanceMeterBuilder) where TClass : class
        {
            return performanceMeterBuilder.Start();
        }

        #endregion
    }
}
