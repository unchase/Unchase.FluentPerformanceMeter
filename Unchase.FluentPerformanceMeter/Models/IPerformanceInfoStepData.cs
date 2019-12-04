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
        /// Step call start date.
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// Step call end date.
        /// </summary>
        DateTime EndTime { get; }

        /// <summary>
        /// Custom data.
        /// </summary>
        IDictionary<string, object> CustomData { get; }
    }
}
