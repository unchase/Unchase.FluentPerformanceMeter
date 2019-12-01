using System;

namespace Unchase.FluentPerformanceMeter
{
    /// <summary>
    /// Performace meter step information.
    /// </summary>
    public interface IPerformanceInfoStepData
    {
        /// <summary>
        /// Step name.
        /// </summary>
        string StepName { get; }

        /// <summary>
        /// Performance meter step call duration.
        /// </summary>
        TimeSpan Elapsed { get; }
    }
}
