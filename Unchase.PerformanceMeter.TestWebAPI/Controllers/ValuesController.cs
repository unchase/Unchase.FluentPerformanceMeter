using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Unchase.PerformanceMeter.TestWebAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [SwaggerTag("Unchase.PerformanceMeter Test WebAPI Controller")]
    public class ValuesController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        static ValuesController()
        {
            PerformanceMeter<ValuesController>.SetMethodCallsCacheTime(5);
            PerformanceMeter<ValuesController>.AddCustomData("Tag", "CustomTag");
            PerformanceMeter<ValuesController>.AddCustomData("Custom anonymous class", new { Name = "Custom Name", Value = 1 });
        }

        public ValuesController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Get methods performance info.
        /// </summary>
        /// <returns>Returns methods performance info.</returns>
        /// <response code="200">Returns methods performance info.</response>
        [HttpGet("GetPerformanceInfo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(SwaggerRequestResponseExamples.GetPerformanceInfoResponse200Example))]
        [IgnoreMethodPerformance]
        public ActionResult<IPerformanceInfo> GetPerformanceInfo()
        {
            return Ok(PerformanceMeter<ValuesController>.GetPerformanceInfo());
        }

        [HttpGet("TestGet")]
        public ActionResult<string> PublicTestGetMethod(uint value)
        {
            var testClass = new
            {
                TestInternalClass = new
                {
                    Key = 1,
                    Value = "2"
                },
                Value = "3"
            };
            using (PerformanceMeter<ValuesController>
                .Watching(nameof(PublicTestGetMethod))
                .WithHttpContextAccessor(_httpContextAccessor)
                .WithCustomData(nameof(value), value)
                .WithCustomData(nameof(testClass), testClass)
                .Start())
            {
                return $"value-{value}";
            }
        }

        [HttpPost("TestPost")]
        public ActionResult<string> PublicPostMethod([FromBody] string value)
        {
            using (PerformanceMeter<ValuesController>
                .Watching(nameof(PublicPostMethod))
                .WithCaller("Test caller")
                .Start())
            {
                return Ok(value);
            }
        }
    }
}
