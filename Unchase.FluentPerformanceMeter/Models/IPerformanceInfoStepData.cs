using System;
using System.Collections.Generic;

namespace Unchase.FluentPerformanceMeter.Models
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

        /// <summary>
        /// Custom data.
        /// </summary>
        IDictionary<string, object> CustomData { get; }
    }
}
