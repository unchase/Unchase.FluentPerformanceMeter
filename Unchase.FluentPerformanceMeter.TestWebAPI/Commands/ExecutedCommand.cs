using System.Diagnostics;

namespace Unchase.FluentPerformanceMeter.TestWebAPI.Commands
{
    /// <summary>
    /// Custom executed command.
    /// </summary>
    internal class ExecutedCommand : IPerformanceCommand
    {
        /// <summary>
        /// Executed commad name.
        /// </summary>
        public string CommandName => this.GetType().Name;

        private string _customString { get; }

        internal bool IsCommandExecuted { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <remarks>
        /// You can pass any data through the command constructor.
        /// </remarks>
        /// <param name="customString"></param>
        public ExecutedCommand(string customString) 
        {
            this._customString = customString;
        }

        /// <summary>
        /// Execute command.
        /// </summary>
        /// <param name="performanceInfo"><see cref="IPerformanceInfo"/>.</param>
        public void Execute(IPerformanceInfo performanceInfo)
        {
            // for example, write to the debug console some information
            Debug.WriteLine(this.CommandName);
            Debug.WriteLine(this._customString);
            Debug.WriteLine($"Method names count: {performanceInfo.MethodNames.Count}");
            this.IsCommandExecuted = true;
        }
    }
}
