using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Unchase.FluentPerformanceMeter
{
    /// <summary>
    /// Class for starting and stopping method steps performance watching.
    /// </summary>
    /// <typeparam name="TClass">Class with methods.</typeparam>
    public class PerformanceInfoStep<TClass> : IDisposable where TClass : class
    {
        #region Peroperties and fields

        private bool _disposed;

        private Stopwatch _innerStopwatch { get; set; } = new Stopwatch();

        private PerformanceMeter<TClass> _performanceMeter { get; }

        private string _stepName { get; }

        private ConcurrentDictionary<string, object> _customData { get; } = new ConcurrentDictionary<string, object>();

        #endregion

        #region Constructors and destructor

        /// <summary>
        /// Private constructor for <see cref="PerformanceInfoStep{TClass}"/>.
        /// </summary>
        private PerformanceInfoStep(PerformanceMeter<TClass> performanceMeter, string stepName)
        {
            this._performanceMeter = performanceMeter;
            this._stepName = stepName;
            this._innerStopwatch.Start();
        }

        /// <summary>
        /// Static constructor for <see cref="PerformanceInfoStep{TClass}"/>.
        /// </summary>
        static PerformanceInfoStep() { }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        /// <summary>
        /// Destructor for <see cref="PerformanceInfoStep{TClass}"/>.
        /// </summary>
        ~PerformanceInfoStep()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion

        #region Main methods

        /// <summary>
        /// Create an instance of the class to watching method step performance.
        /// </summary>
        /// <param name="performanceMeter"><see cref="PerformanceMeter{TClass}"/>.</param>
        /// <param name="stepName">Step name.</param>
        /// <returns>
        /// Returns <see cref="PerformanceInfoStep{TClass}"/>.
        /// </returns>
        internal static PerformanceInfoStep<TClass> WatchingStep(PerformanceMeter<TClass> performanceMeter, string stepName)
        {
            return new PerformanceInfoStep<TClass>(performanceMeter, stepName);
        }

        /// <summary>
        /// Add custom data to performance meter step information.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <returns>
        /// Returns <see cref="PerformanceInfoStep{TClass}"/>.
        /// </returns>
        internal PerformanceInfoStep<TClass> AddCustomData(string key, object value)
        {
            this._customData.TryAdd(key, value);
            return this;
        }

        /// <summary>
        /// Implement IDisposable.
        /// </summary>
        /// <remarks>
        /// Stop watching method step performance.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._disposed)
            {
                if (disposing)
                {
                    if (this._innerStopwatch?.IsRunning == true)
                    {
                        this._innerStopwatch.Stop();
                        this._performanceMeter.Steps.Add(PerformanceInfoStepData.Create(this._stepName, this._innerStopwatch.Elapsed, this._customData));
                        this._innerStopwatch = null;
                    }
                }
            }
            this._disposed = true;
        }

        #endregion
    }

    /// <summary>
    /// Extension methods for <see cref="PerformanceInfoStep{TClass}"/>.
    /// </summary>
    public static class PerformanceInfoStepExtensions
    {
        #region Extension methods

        /// <summary>
        /// Add custom data to performance meter step information.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <param name="performanceInfoStep"><see cref="PerformanceInfoStep{TClass}"/></param>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <returns>
        /// Returns <see cref="PerformanceInfoStep{TClass}"/>.
        /// </returns>
        public static PerformanceInfoStep<TClass> WithCustomData<TClass>(this PerformanceInfoStep<TClass> performanceInfoStep, string key, object value) where TClass : class
        {
            return performanceInfoStep.AddCustomData(key, value);
        }

        #endregion
    }
}
