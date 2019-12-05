namespace Unchase.FluentPerformanceMeter.Models
{
    /// <summary>
    /// Custom executed command interface.
    /// </summary>
    public interface IPerformanceCommand
    {
        /// <summary>
        /// Executed command name.
        /// </summary>
        string CommandName { get; }

        /// <summary>
        /// Execute command.
        /// </summary>
        /// <param name="performanceInfo"><see cref="IPerformanceInfo"/>.</param>
        void Execute(IPerformanceInfo performanceInfo);
    }
}
