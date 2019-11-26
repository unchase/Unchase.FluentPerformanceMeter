using System.Diagnostics;

namespace Unchase.PerformanceMeter.TestWebAPI.Commands
{
    /// <summary>
    /// Custom executed command which work with method calls custom data.
    /// </summary>
    public class CustomDataCommand : IPerformanceCommand
    {
        /// <summary>
        /// Executed commad name.
        /// </summary>
        public string CommandName => this.GetType().Name;

        /// <summary>
        /// Execute command.
        /// </summary>
        /// <param name="performanceInfo"><see cref="IPerformanceInfo"/>.</param>
        public void Execute(IPerformanceInfo performanceInfo)
        {
            foreach (var methodCall in performanceInfo.MethodCalls)
            {
                foreach (var customData in methodCall.CustomData)
                {
                    Debug.WriteLine($"{customData.Key} : {customData.Value}");
                }
            }
        }
    }
}
