using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using Unchase.FluentPerformanceMeter.AspNetCore.Mvc.Attributes;
using Unchase.FluentPerformanceMeter.Builders;
using Unchase.FluentPerformanceMeter.AspNetCore.Mvc.Common;

namespace Unchase.FluentPerformanceMeter.AspNetCore.Mvc
{
    /// <summary>
    /// Represents a middleware that starts and stops an Unchase.FluentPerformanceMeter.
    /// </summary>
    public class PerformanceMeterMiddleware<TClass> where TClass : ControllerBase
    {
        #region Fields

        private readonly RequestDelegate _next;

        private readonly IOptions<PerformanceMeterMvcOptions<TClass>> _options;

        internal PerformanceMeterMvcOptions<TClass> Options => _options.Value;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="PerformanceMeterMiddleware{TClass}"/>
        /// </summary>
        /// <param name="next">The delegate representing the next middleware in the request pipeline.</param>
        /// <param name="options">The middleware options, containing the rules to apply.</param>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="next"/>, or <paramref name="options"/> is <c>null</c>.</exception>
        public PerformanceMeterMiddleware(
            RequestDelegate next,
            IOptions<PerformanceMeterMvcOptions<TClass>> options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes the Unchase.FluentPerformanceMeter-wrapped middleware.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
        /// <returns>A task that represents the execution of the MiniProfiler-wrapped middleware.</returns>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="context"/> is <c>null</c>.</exception>
        public async Task Invoke(HttpContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            if (PerformanceMeterCommonMethods.ShouldWatching(context.Request, Options))
            {
                var controllerActionDescriptor = (context.Features[typeof(IEndpointFeature)] as IEndpointFeature)?.Endpoint?.Metadata?.GetMetadata<ControllerActionDescriptor>();
                if (controllerActionDescriptor != null)
                {
                    if (PerformanceMeterCommonMethods.CheckExcludedMethods(controllerActionDescriptor, Options))
                    {
                        await _next(context); // don't watching, only relay
                    }

                    if (PerformanceMeterCommonMethods.CheckAnnotatedAttribute(controllerActionDescriptor, Options,
                        typeof(WatchingPerformanceAttribute)))
                    {
                        await _next(context); // don't watching, only relay
                    }

                    if (PerformanceMeterCommonMethods.CheckIgnoreMethodPerformanceAttribute(controllerActionDescriptor, Options))
                    {
                        await _next(context); // don't watching, only relay
                    }

                    if (controllerActionDescriptor.ControllerTypeInfo.UnderlyingSystemType != typeof(TClass))
                    {
                        await _next(context); // don't watching, only relay
                    }

                    var performanceMeterBuilder = PerformanceMeter<TClass>
                        .WatchingMethod(controllerActionDescriptor.ActionName)
                        .WithSettingData;

                    // add custom data from custom attributes
                    performanceMeterBuilder =
                        performanceMeterBuilder.AddCustomDataFromCustomAttributes(context, controllerActionDescriptor,
                            Options);

                    using (performanceMeterBuilder.Start())
                    {
                        // execute the pipe
                        await _next(context);
                    }
                }
                else
                {
                    // don't watching, only relay
                    await _next(context);
                }
            }
            else
            {
                // don't watching, only relay
                await _next(context);
            }
        }

        #endregion
    }
}
