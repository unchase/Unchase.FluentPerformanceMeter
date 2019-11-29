using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Unchase.PerformanceMeter.Attributes;
using Unchase.PerformanceMeter.Builders;
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
            PerformanceMeter<ValuesController>.SetDefaultExceptionHandler((ex) => Debug.WriteLine(ex.Message));
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
            return Ok(PerformanceMeter<ValuesController>.PerformanceInfo);
        }

        /// <summary>
        /// Test GET method with simple watching.
        /// </summary>
        [HttpGet("TestGetSimple")]
        public ActionResult PublicTestGetSimpleMethod()
        {
            //using var pm = PerformanceMeter<ValuesController>.WatchingMethod().Start();
            using (PerformanceMeter<ValuesController>.WatchingMethod().Start())
            {
                // Place your code with some logic there

                return Ok();
            }
        }

        /// <summary>
        /// Test GET method with simple watching and executing some code (Action) with trows the exception.
        /// </summary>
        [HttpGet("TestGetSimpleWithActionThrowsException")]
        public ActionResult PublicTestGetSimpleMethodWithActionThrowsException()
        {
            //using var pm = PerformanceMeter<ValuesController>.WatchingMethod().Start();
            using (var pm = PerformanceMeter<ValuesController>.WatchingMethod().Start())
            {
                // Place your code with some logic there

                pm.ExecuteWithExceptionHandling<ValuesController, Exception>(() => throw new Exception("Action exception!!!"));
                return Ok();
            }
        }

        /// <summary>
        /// Test GET method with simple watching and executing some code (Action) without watching.
        /// </summary>
        [HttpGet("TestGetSimpleWithoutWatchingBlock")]
        public ActionResult PublicTestGetSimpleWithoutWatchingBlock()
        {
            //using var pm = PerformanceMeter<ValuesController>.WatchingMethod().Start();
            using (var pm = PerformanceMeter<ValuesController>.WatchingMethod().Start())
            {
                // Place your code with some logic there
                Thread.Sleep(1000);

                pm.ExecuteWithoutWatching(() => Thread.Sleep(2000));
                return Ok();
            }
        }

        /// <summary>
        /// Test GET method with simple watching and executing some code (Action) with trows the custom exception.
        /// </summary>
        [HttpGet("TestGetSimpleWithActionThrowsCustomException")]
        public ActionResult PublicTestGetSimpleMethodWithActionThrowsCustomException()
        {
            //using var pm = PerformanceMeter<ValuesController>.WatchingMethod().Start();
            using (var pm = PerformanceMeter<ValuesController>.WatchingMethod().Start())
            {
                // Place your code with some logic there

                pm.ExecuteWithExceptionHandling<ValuesController, CustomException>(
                    () => throw new CustomException("Action exception!!!"),
                    (_) => Debug.WriteLine("AAAAAAAAA!!!!!!!"));
                return Ok();
            }
        }

        /// <summary>
        /// Test GET method for another class with public method.
        /// </summary>
        /// <returns>
        /// Returns current method calls count before performance watching complete.
        /// </returns>
        [HttpGet("TestGetAnother")]
        public ActionResult<long> PublicTestGetAnotherMethod()
        {
            using (PerformanceMeter<Thread>.WatchingMethod(nameof(Thread.Sleep)).Start())
            {
                return Ok(PerformanceMeter<Thread>.PerformanceInfo.CurrentActivity.FirstOrDefault(ta => ta.MethodName == nameof(Thread.Sleep))?.CallsCount);
            }
        }

        /// <summary>
        /// Call method with for from 0 to 999999.
        /// </summary>
        [HttpGet("CallFor1to1000000")]
        public ActionResult CallFor1to1000000()
        {
            for (int i = 0; i < 1000000; i++)
            {
                var t = i.ToString() + (i + 1).ToString();
            }
            return Ok();
        }

        /// <summary>
        /// Call method with Tread.Sleep(1000).
        /// </summary>
        [HttpGet("CallThreadSleep1000")]
        public ActionResult CallThreadSleep1000()
        {
            Thread.Sleep(1000);
            return Ok();
        }

        /// <summary>
        /// Call method with Tread.Sleep(3000).
        /// </summary>
        [HttpGet("CallThreadSleep3000")]
        public ActionResult CallThreadSleep3000()
        {
            Thread.Sleep(3000);
            return Ok();
        }

        /// <summary>
        /// Call method with steps.
        /// </summary>
        /// <returns>
        /// Return elapsed total milliseconds for all steps.
        /// </returns>
        [HttpGet("TestGetSteps")]
        public ActionResult<long> PublicTestGetSteps()
        {
            var correlationId = Guid.NewGuid();
            using (PerformanceMeter<ValuesController>
                .WatchingMethod()
                .WithSetting
                    .CustomData("corellationId", correlationId)
                    .CallerData()
                .Start())
            {
                using (PerformanceMeter<ValuesController>
                .WatchingMethod(nameof(CallFor1to1000000))
                .WithSetting
                    .CustomData("corellationId", correlationId)
                    .CustomData("step", 1)
                    .CallerData()
                .Start())
                {
                    CallFor1to1000000();
                }

                using (PerformanceMeter<ValuesController>
                    .WatchingMethod(nameof(CallThreadSleep1000))
                    .WithSetting
                        .CustomData("corellationId", correlationId)
                        .CustomData("step", 2)
                        .CallerData()
                    .Start())
                {
                    CallThreadSleep1000();
                }

                using (PerformanceMeter<ValuesController>
                    .WatchingMethod(nameof(CallThreadSleep3000))
                    .WithSetting
                        .CustomData("corellationId", correlationId)
                        .CustomData("step", 3)
                        .CallerData()
                    .Start())
                {
                    CallThreadSleep3000();
                }
            }

            return Ok(PerformanceMeter<ValuesController>.PerformanceInfo.MethodCalls.Where(mc => mc.MethodName.StartsWith("Step")).Sum(mc => mc.Elapsed.TotalMilliseconds));
        }

        /// <summary>
        /// Test GET method with using HttpContextAccessor and adding custom data.
        /// </summary>
        /// <remarks>
        /// With executed command.
        /// </remarks>
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
                .WatchingMethod(nameof(PublicTestGetMethod))
                .WithSetting
                    .CallerFrom(_httpContextAccessor)
                    .CallerData()
                    .CustomData(nameof(value), value)
                    .CustomData(nameof(testClass), testClass)
                .WithExecutingOnComplete
                    .Command(new CustomDataCommand())
                    .Action((pi) =>
                    {
                        Debug.WriteLine($"Class name: {pi.ClassName}");
                    })
                .Start())
            {
                return Ok($"value-{value}");
            }
        }

        /// <summary>
        /// Test POST method with caller name and executed command.
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
            using (var pm = PerformanceMeter<ValuesController>
                .WatchingMethod()
                .WithSetting
                    .CallerFrom("Test caller")
                .WithExecutingOnComplete
                    .Command(new ExecutedCommand("bla-bla-bla"))
                .Start())
            {
                pm.StopWatching(); // stop watching there (or you can use "pm.Dispose();")
                Thread.Sleep(2000);

                return Ok(value);
            }
        }
    }
}
