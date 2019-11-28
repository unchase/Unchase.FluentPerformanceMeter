using System;

namespace Unchase.PerformanceMeter.Builders
{
    /// <summary>
    /// Class for setting custom data for <see cref="PerformanceMeter{TClass}"/>.
    /// </summary>
    /// <typeparam name="TClass">Class with methods.</typeparam>
    public sealed class ExecutedCommandsBuilder<TClass> : PerformanceMeterBuilder<TClass> where TClass : class
    {
        #region Constructors

        /// <summary>
        /// Constructor for <see cref="ExecutedCommandsBuilder{TClass}"/>.
        /// </summary>
        /// <param name="performanceMeter"><see cref="PerformanceMeter{TClass}"/>.</param>
        public ExecutedCommandsBuilder(PerformanceMeter<TClass> performanceMeter)
        {
            this.PerformanceMeter = performanceMeter;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Register commands which will be executed after the performance watching is completed.
        /// </summary>
        /// <param name="performanceCommands">Collection of the executed commands.</param>
        /// <returns>
        /// Returns <see cref="ExecutedCommandsBuilder{TClass}"/>.
        /// </returns>
        internal ExecutedCommandsBuilder<TClass> AddCommands(params IPerformanceCommand[] performanceCommands)
        {
            foreach (var performanceCommand in performanceCommands)
            {
                if (!this.PerformanceMeter.RegisteredCommands.Contains(performanceCommand))
                    this.PerformanceMeter.RegisteredCommands.Add(performanceCommand);
            }
            return this;
        }

        /// <summary>
        /// Register actions which will be executed after the performance watching is completed.
        /// </summary>
        /// <param name="performanceActions">Collection of the executed actions.</param>
        /// <returns>
        /// Returns <see cref="ExecutedCommandsBuilder{TClass}"/>.
        /// </returns>
        internal ExecutedCommandsBuilder<TClass> AddActions(params Action<IPerformanceInfo>[] performanceActions)
        {
            foreach (var performanceAction in performanceActions)
            {
                if (!this.PerformanceMeter.RegisteredActions.Contains(performanceAction))
                    this.PerformanceMeter.RegisteredActions.Add(performanceAction);
            }
            return this;
        }

        #endregion
    }

    /// <summary>
    /// Extension methods for the <see cref="PerformanceMeterBuilder{TClass}"/>
    /// </summary>
    public static class ExecutedCommandsBuilderExtensions
    {
        #region Extension methods

        /// <summary>
        /// Register commands which will be executed after the performance watching is completed.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="executedCommandsBuilder"><see cref="ExecutedCommandsBuilder{TClass}"/>.</param>
        /// <param name="performanceCommands">Collection of the executed commands.</param>
        /// <returns>
        /// Returns <see cref="ExecutedCommandsBuilder{TClass}"/>.
        /// </returns>
        public static ExecutedCommandsBuilder<TClass> Commands<TClass>(this ExecutedCommandsBuilder<TClass> executedCommandsBuilder, params IPerformanceCommand[] performanceCommands) where TClass : class
        {
            return executedCommandsBuilder.AddCommands(performanceCommands);
        }

        /// <summary>
        /// Register command which will be executed after the performance watching is completed.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="executedCommandsBuilder"><see cref="ExecutedCommandsBuilder{TClass}"/>.</param>
        /// <param name="performanceCommand">Executed command.</param>
        /// <returns>
        /// Returns <see cref="ExecutedCommandsBuilder{TClass}"/>.
        /// </returns>
        public static ExecutedCommandsBuilder<TClass> Command<TClass>(this ExecutedCommandsBuilder<TClass> executedCommandsBuilder, IPerformanceCommand performanceCommand) where TClass : class
        {
            return executedCommandsBuilder.AddCommands(performanceCommand);
        }

        /// <summary>
        /// Register actions which will be executed after the performance watching is completed.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="executedCommandsBuilder"><see cref="ExecutedCommandsBuilder{TClass}"/>.</param>
        /// <param name="performanceActions">Collection of the executed actions.</param>
        /// <returns>
        /// Returns <see cref="ExecutedCommandsBuilder{TClass}"/>.
        /// </returns>
        public static ExecutedCommandsBuilder<TClass> Action<TClass>(this ExecutedCommandsBuilder<TClass> executedCommandsBuilder, params Action<IPerformanceInfo>[] performanceActions) where TClass : class
        {
            return executedCommandsBuilder.AddActions(performanceActions);
        }

        /// <summary>
        /// Register action which will be executed after the performance watching is completed.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="executedCommandsBuilder"><see cref="ExecutedCommandsBuilder{TClass}"/>.</param>
        /// <param name="performanceAction">Executed action.</param>
        /// <returns>
        /// Returns <see cref="ExecutedCommandsBuilder{TClass}"/>.
        /// </returns>
        public static ExecutedCommandsBuilder<TClass> Action<TClass>(this ExecutedCommandsBuilder<TClass> executedCommandsBuilder, Action<IPerformanceInfo> performanceAction) where TClass : class
        {
            return executedCommandsBuilder.AddActions(performanceAction);
        }

        #endregion
    }
}
