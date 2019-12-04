using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Unchase.FluentPerformanceMeter.Attributes;
using Unchase.FluentPerformanceMeter.Builders;
using Unchase.FluentPerformanceMeter.Models;
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
            // set cache time
            PerformanceMeter<ValuesController>.SetMethodCallsCacheTime(5);

            // add common custom data (string) to class performance information
            PerformanceMeter<ValuesController>.AddCustomData("Tag", "CustomTag");

            // add common custom data (anonymous class) to class performance information
            PerformanceMeter<ValuesController>.AddCustomData("Custom anonymous class", new { Name = "Custom Name", Value = 1 });

            // set default exception handler
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

        /// <summary>
        /// Get methods performance info for fake service.
        /// </summary>
        /// <returns>Returns fake service methods performance info.</returns>
        /// <response code="200">Returns fake service methods performance info.</response>
        [HttpGet("GetFakeServicePerformanceInfo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseExamples.GetPerformanceInfoResponse200Example))]
        [IgnoreMethodPerformance]
        public ActionResult<IPerformanceInfo> GetFakeServicePerformanceInfo()
        {
            return Ok(PerformanceMeter<FakeService>.PerformanceInfo);
        }

        #endregion

        #region Simple

        /// <summary>
        /// Test GET method with simple performance watching.
        /// </summary>
        [HttpGet("SimpleWatchingMethodStart")]
        public ActionResult SimpleWatchingMethodStart()
        {
            //using var pm = PerformanceMeter<ValuesController>.WatchingMethod().Start();
            using (PerformanceMeter<ValuesController>.WatchingMethod().Start())
            {
                // Place your code with some logic there

                return Ok();
            }
        }

        /// <summary>
        /// Test GET method with simple performance watching.
        /// </summary>
        [HttpGet("SimpleStartWatching")]
        public ActionResult SimpleStartWatching()
        {
            //using var pm = PerformanceMeter<ValuesController>.StartWatching();
            using (PerformanceMeter<ValuesController>.StartWatching())
            {
                // Place your code with some logic there

                return Ok();
            }
        }

        /// <summary>
        /// Test GET method with simple performance watching (with steps).
        /// </summary>
        [HttpGet("SimpleStartWatchingWithSteps")]
        public ActionResult SimpleStartWatchingWithSteps()
        {
            //using var pm = PerformanceMeter<ValuesController>.StartWatching();
            using (var pm = PerformanceMeter<ValuesController>.StartWatching())
            {
                // Place your code with some logic there

                // add "Step 1"
                using (pm.Step("Step 1"))
                {
                    Thread.Sleep(1000);
                }

                // skip this step with minSavems
                using (pm.StepIf("Skipped step", minSaveMs: 100))
                {
                    Thread.Sleep(10);
                }

                // ignore this block in performance wathing
                using (pm.Ignore())
                {
                    Thread.Sleep(5000);
                }

                // add "Step 2" with custom data
                using (var pmStep = pm.Step("Step2 ").WithCustomData("step2 custom data", "data!"))
                {
                    // add "Step 3 in Step 2"
                    using (pm.Step("Step 3 in Step 2"))
                    {
                        Thread.Sleep(1000);
                    }

                    // execute action without performance watching
                    pm.Executing().WithoutWatching().Start(() => Thread.Sleep(2000));

                    // add custom data to "Step 2"
                    pmStep.WithCustomData("step2 another custom data", "data2!");
                }

                return Ok();
            }
        }

        /// <summary>
        /// Test GET method with simple performance watching (with executing some code (Action) throws the exception) with exception handler.
        /// </summary>
        [HttpGet("SimpleStartWatchingWithActionThrowsException")]
        public ActionResult PublicTestGetSimpleMethodWithActionThrowsException()
        {
            //using var pm = PerformanceMeter<ValuesController>.StartWatching();
            using (var pm = PerformanceMeter<ValuesController>.StartWatching())
            {
                // execute action throws Exception with exception handler
                pm.Executing()
                    .WithExceptionHandler((ex) => Debug.WriteLine(ex.Message))
                    .Start(() => throw new Exception(PerformanceMeter<ValuesController>.Print()));

                return Ok();
            }
        }

        /// <summary>
        /// Test GET method with simple performance watching (with executing some code (Action and Func{string}) without performance watching).
        /// </summary>
        [HttpGet("SimpleStartWatchingWithoutWatching")]
        public ActionResult<string> SimpleStartWatchingWithoutWatching()
        {
            //using var pm = PerformanceMeter<ValuesController>.StartWatching();
            using (var pm = PerformanceMeter<ValuesController>.StartWatching())
            {
                // Place your code with some logic there

                // sleep 1 sec
                Thread.Sleep(1000);

                // execute action without watching
                pm.Executing()
                    .WithoutWatching()
                    .Start(() => Thread.Sleep(2000));

                // execute Func<string> returns string
                var result = pm.Executing()
                    .Start(() =>
                    {
                        Thread.Sleep(2000);
                        return "1";
                    });

                return Ok(result);
            }
        }

        /// <summary>
        /// Test GET method with simple performance watching (with executing some code (Action) throws the custom exception).
        /// </summary>
        [HttpGet("SimpleStartWatchingWithActionThrowsCustomException")]
        public ActionResult SimpleStartWatchingWithActionThrowsCustomException()
        {
            //using var pm = PerformanceMeter<ValuesController>.StartWatching();
            using (var pm = PerformanceMeter<ValuesController>.StartWatching())
            {
                // Place your code with some logic there

                // execute action throws custom Exception with exception handler
                pm.Executing<CustomException>()
                   .WithExceptionHandler((ex) => { Debug.WriteLine("Custom exception was occured!"); })
                   .Start(() =>
                   {
                       throw new CustomException("Action exception!!!");
                   });

                return Ok();
            }
        }

        #endregion

        #region Thread.Sleep

        /// <summary>
        /// Test GET method for public method <see cref="Thread.Sleep(int)"/> of the public class <see cref="Thread"/>.
        /// </summary>
        /// <returns>
        /// Returns current method calls count before performance watching complete.
        /// </returns>
        [HttpGet("GetThreadSleepPerformance")]
        public ActionResult<long> GetThreadSleepPerformance()
        {
            using (PerformanceMeter<Thread>.WatchingMethod(nameof(Thread.Sleep)).Start())
            {
                return Ok(PerformanceMeter<Thread>.PerformanceInfo.CurrentActivity.FirstOrDefault(ta => ta.MethodName == nameof(Thread.Sleep))?.CallsCount);
            }
        }

        #endregion

        #region With fake service methods calling steps

        /// <summary>
        /// Call method with calling fake service methods in steps.
        /// </summary>
        /// <returns>
        /// Return elapsed total milliseconds for all steps.
        /// </returns>
        [HttpGet("WatchingMethodStartWithCorrelationIdAndFakeServiceSteps")]
        public ActionResult<long> WatchingMethodStartWithCorrelationIdAndFakeServiceSteps()
        {
            // generate correlationId (for example)
            var correlationId = Guid.NewGuid();

            // start performance watching with correlationId and caller source data
            using (PerformanceMeter<ValuesController>
                .WatchingMethod()
                .WithSettingData
                    .CustomData("corellationId", correlationId)
                    .CallerSourceData()
                .Start())
            {
                using (PerformanceMeter<FakeService>
                .WatchingMethod(nameof(FakeService.FakeMethod1))
                .WithSettingData
                    .CustomData("corellationId", correlationId)
                    .CustomData("fake service method 1 step", 1)
                    .CallerSourceData()
                .Start())
                {
                    FakeService.FakeMethod1();
                }

                using (PerformanceMeter<FakeService>
                    .WatchingMethod(nameof(FakeService.FakeMethod2))
                    .WithSettingData
                        .CustomData("corellationId", correlationId)
                        .CustomData("fake service method 2 step", 2)
                        .CallerSourceData()
                    .Start())
                {
                    FakeService.FakeMethod2();
                }
            }

            return Ok(PerformanceMeter<FakeService>.PerformanceInfo.MethodCalls.Where(mc => mc.MethodName.StartsWith("Fake")).Sum(mc => mc.Elapsed.TotalMilliseconds));
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
        [HttpGet("WatchingMethodWithExecutedCommands")]
        public ActionResult<string> WatchingMethodWithExecutedCommands(uint value)
        {
            // create custom data (anonymous class)
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
                .WatchingMethod(nameof(WatchingMethodWithExecutedCommands))
                .WithSettingData
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
        /// Test POST method with caller name and custom data (from attribute) and executed command.
        /// </summary>
        /// <param name="value">Some value from body.</param>
        /// <returns>
        /// Returns input value.
        /// </returns>
        [HttpPost("StartWatchingWithCallerName")]
        [MethodCustomData("customData123", 123)]
        public ActionResult<string> StartWatchingWithCallerName([FromBody] string value)
        {
            // method performance info will reach with caller name (if internal HttpContextAccessor is null)
            using (var pm = PerformanceMeter<ValuesController>
                .WatchingMethod()
                .WithSettingData
                    .CallerFrom("Test caller")
                .Start())
            {
                pm.StopWatching(); // stop watching there (or you can use "pm.Dispose();")
                Thread.Sleep(2000);

                return Ok(value);
            }
        }

        /// <summary>
        /// Test POST method with caller (from attribute) name and executed command.
        /// </summary>
        /// <param name="value">Some value from body.</param>
        /// <returns>
        /// Returns input value.
        /// </returns>
        [HttpPost("StartWatchingWithCallerNameFromAttribute")]
        [MethodCaller("testCaller")]
        public ActionResult<string> StartWatchingWithCallerNameFromAttribute([FromBody] string value)
        {
            // method performance info will reach with caller name (if internal HttpContextAccessor is null)
            using (var pm = PerformanceMeter<ValuesController>.StartWatching())
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
