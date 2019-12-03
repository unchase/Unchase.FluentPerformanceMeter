using System;
using System.Collections.Generic;

namespace Unchase.FluentPerformanceMeter.Models
{
    /// <summary>
    /// Performace meter step information.
    /// </summary>
    internal class PerformanceInfoStepData : IPerformanceInfoStepData
    {
        #region Properties

        /// <summary>
        /// Step name.
        /// </summary>
        public string StepName { get; }

        /// <summary>
        /// Performance meter step call duration.
        /// </summary>
        public TimeSpan Elapsed { get; }

        /// <summary>
        /// Custom data.
        /// </summary>
        public IDictionary<string, object> CustomData { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for <see cref="PerformanceInfoStepData"/>.
        /// </summary>
        /// <param name="stepName">Step name.</param>
        /// <param name="elapsed">Performance meter step call duration.</param>
        /// <param name="customData">Custom data.</param>
        private PerformanceInfoStepData(string stepName, TimeSpan elapsed, IDictionary<string, object> customData)
        {
            this.StepName = stepName;
            this.Elapsed = elapsed;
            this.CustomData = customData;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create performance meter step data class instance.
        /// </summary>
        /// <param name="stepName">Step name.</param>
        /// <param name="elapsed">Performance meter step call duration.</param>
        /// <param name="customData">Custom data.</param>
        /// <returns>
        /// Returns <see cref="PerformanceInfoStepData"/>.
        /// </returns>
        internal static PerformanceInfoStepData Create(string stepName, TimeSpan elapsed, IDictionary<string, object> customData)
        {
            return new PerformanceInfoStepData(stepName, elapsed, customData);
        }

        #endregion
    }
}
