using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using Unchase.PerformanceMeter.TestWebAPI.Commands;
using Unchase.PerformanceMeter.TestWebAPI.SwaggerExamples;

namespace Unchase.PerformanceMeter.TestWebAPI.Controllers
{
    /// <summary>
    /// Unchase.PerformanceMeter Test WebAPI Controller.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("Unchase.PerformanceMeter Test WebAPI Controller")]
    public class ValuesController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static ValuesController()
        {
            PerformanceMeter<ValuesController>.SetMethodCallsCacheTime(5);
            PerformanceMeter<ValuesController>.AddCustomData("Tag", "CustomTag");
            PerformanceMeter<ValuesController>.AddCustomData("Custom anonymous class", new { Name = "Custom Name", Value = 1 });
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="httpContextAccessor"><see cref="IHttpContextAccessor"/>.</param>
        public ValuesController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Get methods performance info for this controller.
        /// </summary>
        /// <returns>Returns methods performance info.</returns>
        /// <response code="200">Returns methods performance info.</response>
        [HttpGet("GetPerformanceInfo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseExamples.GetPerformanceInfoResponse200Example))]
        [IgnoreMethodPerformance]
        public ActionResult<IPerformanceInfo> GetPerformanceInfo()
        {
            return Ok(PerformanceMeter<ValuesController>.GetPerformanceInfo());
        }

        /// <summary>
        /// Test GET method.
        /// </summary>
        /// <param name="value">Some value.</param>
        /// <returns>
        /// Returns input value.
        /// </returns>
        [HttpGet("TestGet")]
        public ActionResult<string> PublicTestGetMethod(uint value)
        {
            // create custom data
            var testClass = new
            {
                TestInternalClass = new
                {
                    Key = 1,
                    Value = "2"
                },
                Value = "3"
            };

            // method performance info will reach with HttpContextAccessor and custom data
            // custom "CustomDataCommand" will be executed after performance watching is completed (work with method calls custom data)
            using (PerformanceMeter<ValuesController>
                .Watching(nameof(PublicTestGetMethod))
                .WithHttpContextAccessor(_httpContextAccessor)
                .WithCustomData(nameof(value), value)
                .WithCustomData(nameof(testClass), testClass)
                .WithExecutingOnComplete(new CustomDataCommand())
                .Start())
            {
                return Ok($"value-{value}");
            }
        }

        /// <summary>
        /// Test POST method.
        /// </summary>
        /// <param name="value">Some value from body.</param>
        /// <returns>
        /// Returns input value.
        /// </returns>
        [HttpPost("TestPost")]
        public ActionResult<string> PublicPostMethod([FromBody] string value)
        {
            // method performance info will reach with caller name (if internal HttpContextAccessor is null)
            // custom "ExecuteCommand" will be executed after performance watching is completed (for example, you can write data to the database or log the result or perform any other operation)
            using (PerformanceMeter<ValuesController>
                .Watching()
                .WithCaller("Test caller")
                .WithExecutingOnComplete(new ExecutedCommand("bla-bla-bla"))
                .Start())
            {
                return Ok(value);
            }
        }
    }
}
