using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Unchase.FluentPerformanceMeter.AspNetCore.Mvc.DiagnosticSource;

namespace Unchase.FluentPerformanceMeter.AspNetCore.Mvc.Extensions
{
    /// <summary>
    /// Extension methods for Diagnostic Source.
    /// </summary>
    public static class PerformanceDiagnosticSourceExtensions
    {
        #region Extension methods

        #region DiagnosticObserver

        /// <summary>
        /// Register <see cref="PerformanceDiagnosticObserverBase"/> services.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/>.</param>
        /// <param name="configureOptions">An <see cref="Action{PerformanceMeterOptions}"/> to configure options for Unchase.FluentPerformanceMeter.</param>
        public static IServiceCollection AddPerformanceDiagnosticObserver<TClass>(this IServiceCollection services, Action<PerformanceMeterMvcOptions<TClass>> configureOptions = null) where TClass : ControllerBase
        {
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }
            services.Configure<PerformanceMeterMvcOptions<TClass>>(o => PerformanceMeter<TClass>.Configure(o));

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<PerformanceDiagnosticObserverBase, PerformanceDiagnosticObserver<TClass>>());

            return services;
        }

        /// <summary>
        /// Use <see cref="PerformanceDiagnosticObserverBase"/> diagnostic source subscriptions.
        /// </summary>
        /// <param name="app"><see cref="IApplicationBuilder"/>.</param>
        public static IApplicationBuilder UsePerformanceDiagnosticObserver(this IApplicationBuilder app)
        {
            var diagnosticObservers = app.ApplicationServices.GetServices<PerformanceDiagnosticObserverBase>();
            foreach (var diagnosticObserver in diagnosticObservers)
            {
                DiagnosticListener.AllListeners.Subscribe(diagnosticObserver);
            }

            return app;
        }

        #endregion

        #region PerformanceMeter

        /// <summary>
        /// Register Unchase.FluentPerformanceMeter services.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/>.</param>
        /// <param name="configureOptions">An <see cref="Action{PerformanceMeterMvcOptions}"/> to configure options for Unchase.FluentPerformanceMeter.</param>
        public static IServiceCollection AddPerformanceMeter<TClass>(this IServiceCollection services, Action<PerformanceMeterMvcOptions<TClass>> configureOptions = null) where TClass : ControllerBase
        {
            // ensure that IHttpContextAccessor was added
            services.AddHttpContextAccessor();

            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }

            services.Configure<PerformanceMeterMvcOptions<TClass>>(o =>
            {
                PerformanceMeter<TClass>.Configure(o);
            });

            services.AddScoped(typeof(PerformanceMeter<TClass>), serviceProvider =>
            {
                var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>()?.HttpContext;
                return httpContext?.Items[$"PerformanceMeter{httpContext.TraceIdentifier}"];
            });

            return services;
        }

        /// <summary>
        /// Use middleware for watching methods performance.
        /// </summary>
        /// <param name="app"><see cref="IApplicationBuilder"/>.</param>
        public static IApplicationBuilder UsePerformanceMeterFor<TClass>(this IApplicationBuilder app) where TClass : ControllerBase
        {
            return app.UseMiddleware<PerformanceMeterMiddleware<TClass>>();
        }

        #endregion

        #endregion
    }
}
