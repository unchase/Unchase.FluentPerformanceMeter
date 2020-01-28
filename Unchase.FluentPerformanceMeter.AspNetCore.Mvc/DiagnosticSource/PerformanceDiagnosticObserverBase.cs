using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Unchase.FluentPerformanceMeter.AspNetCore.Mvc.DiagnosticSource
{
    /// <summary>
    /// The base class for performance diagnostic source.
    /// </summary>
    public abstract class PerformanceDiagnosticObserverBase : IObserver<DiagnosticListener>
    {
        #region Fields

        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        #endregion

        #region Methods

        /// <summary>
        /// Check matching the name.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>
        /// Returns true if name is match.
        /// </returns>
        protected abstract bool IsMatch(string name);

        void IObserver<DiagnosticListener>.OnNext(DiagnosticListener diagnosticListener)
        {
            if (IsMatch(diagnosticListener.Name))
            {
                var subscription = diagnosticListener.SubscribeWithAdapter(this);
                _subscriptions.Add(subscription);
            }
        }

        void IObserver<DiagnosticListener>.OnError(Exception error)
        {
            HandleException(error);
        }

        void IObserver<DiagnosticListener>.OnCompleted()
        {
            _subscriptions.ForEach(x => x.Dispose());
            _subscriptions.Clear();
        }

        /// <summary>
        /// Handle the exception.
        /// </summary>
        /// <param name="error">Exception.</param>
        protected abstract void HandleException(Exception error);

        #endregion
    }
}
