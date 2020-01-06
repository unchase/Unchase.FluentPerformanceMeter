using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics;
using Unchase.FluentPerformanceMeter.AspNetCore.Mvc.DiagnosticSource;

namespace Unchase.FluentPerformanceMeter.AspNetCore.Mvc.Extensions
{
    /// <summary>
    /// Extension methods for Diagnostic Source.
    /// </summary>
    public static class PerformanceDiagnosticSourceExtensions
    {
        #region Extension methods

        /// <summary>
        /// Register <see cref="PerformanceDiagnosticObserverBase"/> services.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/>.</param>
        public static void AddPerformanceDiagnosticObserver<TDiagnosticObserver>(this IServiceCollection services) where TDiagnosticObserver : PerformanceDiagnosticObserverBase
        {
            services.TryAddEnumerable(ServiceDescriptor.Transient<PerformanceDiagnosticObserverBase, TDiagnosticObserver>());
        }

        /// <summary>
        /// Use <see cref="PerformanceDiagnosticObserverBase"/> diagnostic source subscriptions.
        /// </summary>
        /// <param name="app"><see cref="IApplicationBuilder"/>.</param>
        public static void UsePerformanceDiagnosticObserver(this IApplicationBuilder app)
        {
            var diagnosticObservers = app.ApplicationServices.GetServices<PerformanceDiagnosticObserverBase>();
            foreach (var diagnosticObserver in diagnosticObservers)
            {
                DiagnosticListener.AllListeners.Subscribe(diagnosticObserver);
            }
        }

        #endregion
    }
}
