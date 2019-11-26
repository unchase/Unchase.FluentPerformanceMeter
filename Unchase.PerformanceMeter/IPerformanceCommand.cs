namespace Unchase.PerformanceMeter
{
    /// <summary>
    /// Command for executing after stop watching method performance.
    /// </summary>
    public interface IPerformanceCommand
    {
        /// <summary>
        /// Command Name.
        /// </summary>
        string CommandName { get; }

        /// <summary>
        /// Execute command.
        /// </summary>
        /// <param name="performanceInfo"><see cref="IPerformanceInfo"/>.</param>
        void Execute(IPerformanceInfo performanceInfo);
    }
}
