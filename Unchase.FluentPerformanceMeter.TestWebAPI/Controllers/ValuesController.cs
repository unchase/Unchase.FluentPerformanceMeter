using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unchase.FluentPerformanceMeter.Attributes;
using Unchase.FluentPerformanceMeter.Builders;
using Unchase.FluentPerformanceMeter.TestWebAPI.Commands;
using Unchase.FluentPerformanceMeter.TestWebAPI.SwaggerExamples;

namespace Unchase.FluentPerformanceMeter.TestWebAPI.Controllers
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
        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Constructors

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

        #endregion

        #region Actions

        #region Get Performance information

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

        #endregion

        #region Simple

        /// <summary>
        /// Test GET method with performance watching.
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
        /// Test GET method with performance watching (with executing some code (Action) throws the exception).
        /// </summary>
        [HttpGet("TestGetSimpleWithActionThrowsException")]
        public ActionResult PublicTestGetSimpleMethodWithActionThrowsException()
        {
            //using var pm = PerformanceMeter<ValuesController>.WatchingMethod().Start();
            using (var pm = PerformanceMeter<ValuesController>.WatchingMethod().Start())
            {
                // Place your code with some logic there

                pm.Executing()
                    .WithExceptionHandler((ex) => { Trace.WriteLine(ex.Message); })
                    .Start(() => throw new Exception("Action exception!!!"));

                return Ok();
            }
        }

        /// <summary>
        /// Test GET method with performance watching (with executing some code (Action) without performance watching).
        /// </summary>
        [HttpGet("TestGetSimpleWithoutWatching")]
        public ActionResult PublicTestGetSimpleWithoutWatching()
        {
            //using var pm = PerformanceMeter<ValuesController>.WatchingMethod().Start();
            using (var pm = PerformanceMeter<ValuesController>.WatchingMethod().Start())
            {
                // Place your code with some logic there

                Thread.Sleep(1000);

                pm.Executing()
                    .WithoutWatching()
                    .Start(() => { Thread.Sleep(2000); });

                pm.Executing()
                    .Start(() =>
                    {
                        Thread.Sleep(2000);
                        return "1";
                    });

                return Ok();
            }
        }

        /// <summary>
        /// Test GET method with performance watching (with executing some code (Task) without performance watching).
        /// </summary>
        [HttpGet("TestGetSimpleWithoutWatchingTaskAsync")]
        public async Task<ActionResult> PublicTestGetSimpleWithoutWatchingTaskAsync()
        {
            //using var pm = PerformanceMeter<ValuesController>.WatchingMethod().Start();
            using (var pm = PerformanceMeter<ValuesController>.WatchingMethod().Start())
            {
                // Place your code with some logic there

                Thread.Sleep(1000);

                await pm.Executing()
                    .WithoutWatching()
                    .StartAsync(Task.Run(() => Thread.Sleep(2000)));

                return Ok();
            }
        }

        /// <summary>
        /// Test GET method with performance watching (with executing some code (Action) throws the custom exception).
        /// </summary>
        [HttpGet("TestGetSimpleWithActionThrowsCustomException")]
        public ActionResult PublicTestGetSimpleMethodWithActionThrowsCustomException()
        {
            //using var pm = PerformanceMeter<ValuesController>.WatchingMethod().Start();
            using (var pm = PerformanceMeter<ValuesController>.WatchingMethod().Start())
            {
                // Place your code with some logic there

                pm.Executing<CustomException>()
                   .WithExceptionHandler((ex) => { Debug.WriteLine("AAAAAAAAA!!!!!!!"); })
                   .Start(() =>
                   {
                       throw new CustomException("Action exception!!!");
                   });
                return Ok();
            }
        }

        #endregion

        #region From external dll

        /// <summary>
        /// Test GET method for public method <see cref="Thread.Sleep(int)"/> of the <see cref="Thread"/>.
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

        #endregion

        #region With steps

        #region Steps

        /// <summary>
        /// Call method with "for from 0 to 999999".
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
        /// Call method with "Tread.Sleep(1000)".
        /// </summary>
        [HttpGet("CallThreadSleep1000")]
        public ActionResult CallThreadSleep1000()
        {
            Thread.Sleep(1000);
            return Ok();
        }

        /// <summary>
        /// Call method with "Tread.Sleep(3000)".
        /// </summary>
        [HttpGet("CallThreadSleep3000")]
        public ActionResult CallThreadSleep3000()
        {
            Thread.Sleep(3000);
            return Ok();
        }

        #endregion

        /// <summary>
        /// Call method with few steps.
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
                    .CallerSourceData()
                .Start())
            {
                using (PerformanceMeter<ValuesController>
                .WatchingMethod(nameof(CallFor1to1000000))
                .WithSetting
                    .CustomData("corellationId", correlationId)
                    .CustomData("step", 1)
                    .CallerSourceData()
                .Start())
                {
                    CallFor1to1000000();
                }

                using (PerformanceMeter<ValuesController>
                    .WatchingMethod(nameof(CallThreadSleep1000))
                    .WithSetting
                        .CustomData("corellationId", correlationId)
                        .CustomData("step", 2)
                        .CallerSourceData()
                    .Start())
                {
                    CallThreadSleep1000();
                }

                using (PerformanceMeter<ValuesController>
                    .WatchingMethod(nameof(CallThreadSleep3000))
                    .WithSetting
                        .CustomData("corellationId", correlationId)
                        .CustomData("step", 3)
                        .CallerSourceData()
                    .Start())
                {
                    CallThreadSleep3000();
                }
            }

            return Ok(PerformanceMeter<ValuesController>.PerformanceInfo.MethodCalls.Where(mc => mc.MethodName.StartsWith("Step")).Sum(mc => mc.Elapsed.TotalMilliseconds));
        }

        #endregion

        #region With HttpContextAccessor, custom data and executed commands

        /// <summary>
        /// Test GET method with using HttpContextAccessor and adding custom data.
        /// </summary>
        /// <remarks>
        /// With executed command after performance watching.
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
                    .CallerSourceData()
                    .CustomData(nameof(value), value)
                    .CustomData(nameof(testClass), testClass)
                .WithExecutingOnComplete
                    .Command(new CustomDataCommand())
                    .Command(new ExecutedCommand("bla-bla-bla"))
                    .Action((pi) =>
                    {
                        Debug.WriteLine($"Class name: {pi.ClassName}");
                    })
                .Start())
            {
                return Ok($"{value}");
            }
        }

        #endregion

        #region With caller

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
                .Start())
            {
                pm.StopWatching(); // stop watching there (or you can use "pm.Dispose();")
                Thread.Sleep(2000);

                return Ok(value);
            }
        }

        #endregion

        #endregion
    }
}
