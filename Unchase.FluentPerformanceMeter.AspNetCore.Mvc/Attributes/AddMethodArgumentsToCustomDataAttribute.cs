using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Unchase.FluentPerformanceMeter.AspNetCore.Mvc.Attributes
{
    /// <summary>
    /// Attribute to adding action arguments to the measurement custom data with <see cref="WatchingWithDiagnosticSourceAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AddMethodArgumentsToCustomDataAttribute : ActionFilterAttribute
    {
        #region Fileds

        private string _argumentsKey;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for <see cref="AddMethodArgumentsToCustomDataAttribute"/>.
        /// </summary>
        /// <param name="argumentsKey">Key for arguments in custom data storage.</param>
        public AddMethodArgumentsToCustomDataAttribute(string argumentsKey = "arguments")
        {
            this._argumentsKey = argumentsKey;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Call before the action executes, after model binding is complete.
        /// </summary>
        /// <param name="context"><see cref="ActionExecutingContext"/>.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Items.TryGetValue("PerformanceMeter", out object performanceMeter))
            {
                var tryAddCustomDataMethod = performanceMeter.GetType().GetMethod("TryAddCustomData");
                tryAddCustomDataMethod.Invoke(performanceMeter, new object[] { this._argumentsKey, context.ActionArguments });
            }

            base.OnActionExecuting(context);
        }

        #endregion
    }
}
