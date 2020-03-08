using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using Unchase.FluentPerformanceMeter.AspNetCore.Mvc.Attributes;
using Unchase.FluentPerformanceMeter.Attributes;
using Unchase.FluentPerformanceMeter.Builders;
using Unchase.FluentPerformanceMeter.Extensions;
using Unchase.FluentPerformanceMeter.Models;
using Unchase.FluentPerformanceMeter.Test.Common;
using Unchase.FluentPerformanceMeter.Test.Common.Commands;
using Unchase.FluentPerformanceMeter.TestWebAPI31.OpenApiExamples;

namespace Unchase.FluentPerformanceMeter.TestWebAPI31.Controllers
{
    /// <summary>
    /// Unchase.PerformanceMeter Test WebAPI Controller.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("Unchase.PerformanceMeter Test WebAPI Controller")]
    //[WatchingWithDiagnosticSource]
    public class PerformanceMeterController : ControllerBase
    {
        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Constructors

        /// <summary>
        /// Static constructor.
        /// </summary>
        static PerformanceMeterController()
        {
            // set cache time for PerformanceMeterController class
            PerformanceMeter<PerformanceMeterController>.SetMethodCallsCacheTime(5);

            // add common custom data (string) to class performance information
            PerformanceMeter<PerformanceMeterController>.AddCustomData("Tag", "CustomTag");

            // add common custom data (anonymous class) to class performance information
            PerformanceMeter<PerformanceMeterController>.AddCustomData("Custom anonymous class", new { Name = "Custom Name", Value = 1 });

            // set default exception handler for PerformanceMeterController class
            PerformanceMeter<PerformanceMeterController>.SetDefaultExceptionHandler((ex) => Debug.WriteLine(ex.Message));
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="httpContextAccessor"><see cref="IHttpContextAccessor"/>.</param>
        public PerformanceMeterController(IHttpContextAccessor httpContextAccessor)
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
            return Ok(PerformanceMeter<PerformanceMeterController>.PerformanceInfo);
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

        /// <summary>
        /// Get methods performance info for Thread class.
        /// </summary>
        /// <returns>Returns Thread methods performance info.</returns>
        /// <response code="200">Returns Thread methods performance info.</response>
        [HttpGet("GetThreadPerformanceInfo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ResponseExamples.GetPerformanceInfoResponse200Example))]
        [IgnoreMethodPerformance]
        public ActionResult<IPerformanceInfo> GetThreadPerformanceInfo()
        {
            return Ok(PerformanceMeter<Thread>.PerformanceInfo);
        }

        #endregion

        #region Simple

        /// <summary>
        /// Test GET method with simple performance watching.
        /// </summary>
        [HttpGet("SimpleWatchingMethodStart")]
        [WatchingPerformance]
        public ActionResult SimpleWatchingMethodStart()
        {
            using var pm = PerformanceMeter<PerformanceMeterController>.WatchingMethod().Start();

            //put your code with some logic here

            return Ok();
        }

        /// <summary>
        /// Test GET method with simple performance watching.
        /// </summary>
        [HttpGet("SimpleStartWatching")]
        public ActionResult SimpleStartWatching()
        {
            using var pm = PerformanceMeter<PerformanceMeterController>.StartWatching();

            // put your code with some logic here

            return Ok();
        }

        /// <summary>
        /// Test GET method from Func with simple performance watching.
        /// </summary>
        [HttpGet("SimpleStartWatchingFuncMethod")]
        public ActionResult SimpleStartWatchingFuncMethod()
        {
            using var pm = PerformanceMeter<PerformanceMeterController>.StartWatching(SimpleStartWatchingFuncMethod);

            // put your code with some logic here

            return Ok();
        }

        /// <summary>
        /// Test GET method with simple performance watching (with steps).
        /// </summary>
        [HttpGet("SimpleStartWatchingWithSteps")]
        [MethodCustomData("Custom data from attribute", "Attr")]
        public ActionResult SimpleStartWatchingWithSteps()
        {
            using var pm = PerformanceMeter<PerformanceMeterController>
                .WatchingMethod()
                .WithSettingData
                .CustomData("coins", 1)
                .CustomData("Coins sets", new
                {
                    Gold = "Many",
                    Silver = 5
                })
                .Start();

            // put your code with some logic here

            // add "Step 1"
            using (pm.Step("Step 1"))
            {
                Thread.Sleep(1000);
            }

            // skip this step with minSavems (not save, but consider duration in method performance watching)
            using (pm.StepIf("Skipped step", minSaveMs: 100))
            {
                Thread.Sleep(10);
            }

            // ignore this block in performance watching
            using (pm.Ignore())
            {
                Thread.Sleep(5000);
            }

            // add "Step 2" with custom data
            using (var pmStep = pm.Step("Step 2").AddCustomData("step2 custom data", "data!"))
            {
                // add "Step 3 in Step 2"
                using (pm.Step("Step 3 in Step 2"))
                {
                    Thread.Sleep(1000);
                }

                // execute action without performance watching
                pm.Executing().WithoutWatching().Start(() => Thread.Sleep(2000));

                // add custom data to "Step 2"
                pmStep.AddCustomData("step2 another custom data", "data2!");

                // get and remove custom data from "Step 2"
                var customData = pmStep.GetAndRemoveCustomData<string>("step2 custom data");
                Debug.WriteLine($"{customData}!!!");

                // get custom data from "Step 2" (without removing)
                var anotherCustomData = pmStep.GetCustomData<string>("step2 another custom data");
            }

            return Ok();
        }

        /// <summary>
        /// Test GET method with simple performance watching (with ignored blocks).
        /// </summary>
        [HttpGet("SimpleStartWatchingWithIgnored")]
        [MethodCustomData("Custom data from attribute", "Attr")]
        public ActionResult SimpleStartWatchingWithIgnored()
        {
            using var pm = PerformanceMeter<PerformanceMeterController>.StartWatching();

            // put your code with some logic here

            // sleep 1 sec
            Thread.Sleep(1000);

            // ignore this block in performance watching
            using (pm.Ignore())
            {
                Thread.Sleep(5000);
            }

            // skip this step with minSaveMs (do not save the step results, but take into account its duration)
            using (pm.StepIf("Skipped step", minSaveMs: 1000))
            {
                Thread.Sleep(999);
            }

            // execute action without performance watching
            pm.Executing().WithoutWatching().Start(() => Thread.Sleep(2000));

            return Ok();
        }

        /// <summary>
        /// Test GET method with simple performance watching (with executing some code (Action) throws the exception) with exception handler.
        /// </summary>
        [HttpGet("SimpleStartWatchingWithActionThrowsException")]
        public ActionResult PublicTestGetSimpleMethodWithActionThrowsException()
        {
            using var pm = PerformanceMeter<PerformanceMeterController>.StartWatching();

            // execute an action that throws the exception to be handled by the exception handler
            pm.Executing()
                .WithExceptionHandler((ex) => Debug.WriteLine(ex.Message))
                .Start(() => throw new Exception(PerformanceMeter<PerformanceMeterController>.Print()));

            return Ok();
        }

        /// <summary>
        /// Test GET method with simple performance watching (with executing some code (Action and Func{string}) without performance watching).
        /// </summary>
        [HttpGet("SimpleStartWatchingWithoutWatching")]
        public ActionResult<string> SimpleStartWatchingWithoutWatching()
        {
            using var pm = PerformanceMeter<PerformanceMeterController>.StartWatching();

            // put your code with some logic here

            // sleep 1 sec
            Thread.Sleep(1000);

            // execute action without watching
            pm.Executing()
                .WithoutWatching()
                .Start(() => Thread.Sleep(2000));

            // execute action with sleeping 500 ms
            pm.Inline(() =>
            {
                Thread.Sleep(500);
                Debug.WriteLine("Sleep 500 ms");
            });

            // execute action without watching
            pm.InlineIgnored(() =>
            {
                Thread.Sleep(100);
                Debug.WriteLine("Sleep 1000 ms");
            });

            // execute Func<string> returns string
            var result = pm.Executing()
                .Start(() =>
                {
                    Thread.Sleep(2000);
                    return "1";
                });

            return Ok(result);
        }

        /// <summary>
        /// Test GET method with simple performance watching (with executing some code (Action) throws the custom exception).
        /// </summary>
        [HttpGet("SimpleStartWatchingWithActionThrowsCustomException")]
        public ActionResult SimpleStartWatchingWithActionThrowsCustomException()
        {
            using var pm = PerformanceMeter<PerformanceMeterController>.StartWatching();

            // put your code with some logic here

            // execute action throws custom Exception with exception handler
            pm.Executing<CustomException>()
                .WithExceptionHandler((ex) => { Debug.WriteLine("Custom exception was occured!"); })
                .Start(() => throw new CustomException("Action exception!!!"));

            return Ok();
        }

        /// <summary>
        /// Test GET method with simple performance watching (with throws the exceptions) with exception handlers.
        /// </summary>
        [HttpGet("SimpleStartWatchingWithThrowsExceptions")]
        public ActionResult SimpleStartWatchingWithThrowsExceptions()
        {
            using var pm = PerformanceMeter<PerformanceMeterController>.StartWatching();

            // execute an action that throws the exception to be handled by the exception handler
            pm.Executing()
                .WithExceptionHandler((ex) => Debug.WriteLine(ex.Message))
                .Start(() => throw new Exception("Exception"));

            // execute action throws custom Exception with exception handler
            pm.Executing<CustomException>()
                .WithExceptionHandler((ex) => { Debug.WriteLine(ex.Message); })
                .Start(() => throw new CustomException("Custom exception was occured!"));

            return Ok();
        }

        #endregion

        #region Thread.Sleep

        /// <summary>
        /// Test GET method for public method <see cref="Thread.Sleep(int)"/> of the public class <see cref="Thread"/>.
        /// </summary>
        /// <returns>
        /// Returns external method call elapsed time.
        /// </returns>
        [HttpGet("GetThreadSleepPerformance")]
        public ActionResult<string> GetThreadSleepPerformance()
        {
            using (PerformanceMeter<Thread>.WatchingMethod(nameof(Thread.Sleep)).Start())
            {
                Thread.Sleep(1000);
            }

            return Ok(PerformanceMeter<Thread>.PerformanceInfo.MethodCalls.FirstOrDefault(ta => ta.MethodName == nameof(Thread.Sleep))?.Elapsed);
        }

        /// <summary>
        /// Test GET method from Action for public method <see cref="Thread.Sleep(int)"/> of the public class <see cref="Thread"/>.
        /// </summary>
        /// <returns>
        /// Returns external method call elapsed time.
        /// </returns>
        [HttpGet("GetThreadSleepFromActionPerformance")]
        public ActionResult<string> GetThreadSleepFromActionPerformance()
        {
            using (PerformanceMeter<Thread>.WatchingMethod<int>(Thread.Sleep).Start())
            {
                Thread.Sleep(1000);
            }

            return Ok(PerformanceMeter<Thread>.PerformanceInfo.MethodCalls.FirstOrDefault(ta => ta.MethodName == nameof(Thread.Sleep))?.Elapsed);
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

            var parameterForMethod2 = "parameter";

            // start performance watching with correlationId and caller source data
            using (PerformanceMeter<PerformanceMeterController>
                .WatchingMethod()
                .WithSettingData
                    .CustomData("corellationId", correlationId)
                    .CallerSourceData()
                .Start())
            {
                // add step with calling FakeService.FakeMethod2 with custom data (corellationId)
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

                // add step with calling FakeService.FakeMethod2 with custom data (corellationId and perameter for FakeMethod2)
                using (PerformanceMeter<FakeService>
                    .WatchingMethod(nameof(FakeService.FakeMethod2))
                    .WithSettingData
                        .CustomData("corellationId", correlationId)
                        .CustomData("fake service method 2 step", 2)
                        .CustomData("method parameter", parameterForMethod2)
                        .CallerSourceData()
                    .Start())
                {
                    FakeService.FakeMethod2(parameterForMethod2);
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
            using (PerformanceMeter<PerformanceMeterController>
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

        /// <summary>
        /// Test GET method with adding custom data.
        /// </summary>
        /// <remarks>
        /// With executed command after performance watching.
        /// </remarks>
        /// <returns>
        /// Returns input value.
        /// </returns>
        [HttpGet("WatchingMethodWithExecutedCommand")]
        public ActionResult WatchingMethodWithExecutedCommand()
        {
            // custom "ExecutedCommand" will be executed after performance watching is completed
            using (PerformanceMeter<PerformanceMeterController>
                .WatchingMethod()
                .WithExecutingOnComplete
                    .Command(new ExecutedCommand("bla-bla-bla"))
                    .Action((pi) =>
                    {
                        Debug.WriteLine($"Class name: {pi.ClassName}");
                    })
                .Start())
            {
                return Ok();
            }
        }

        #endregion

        #region With caller

        /// <summary>
        /// Test POST method with caller name and custom data (from attribute).
        /// </summary>
        /// <param name="value">Some value from body.</param>
        /// <returns>
        /// Returns input value.
        /// </returns>
        [HttpPost("StartWatchingWithCallerName")]
        [MethodCustomData("customData123", 123)]
        [MethodCaller("testCaller")]
        public ActionResult<string> StartWatchingWithCallerName([FromBody] string value)
        {
            // the method’s performance info will be amended with the caller's name (if internal HttpContextAccessor is null)
            using var pm = PerformanceMeter<PerformanceMeterController>
                .WatchingMethod()
                .WithSettingData
                .CallerSourceData()
                .CallerFrom("Test caller")
                .Start();
            pm.StopWatching(); // stop watching here (or you can use "pm.Dispose();")
            Thread.Sleep(2000);

            return Ok(value);
        }

        /// <summary>
        /// Test POST method with caller name (from attribute).
        /// </summary>
        /// <param name="value">Some value from body.</param>
        /// <returns>
        /// Returns input value.
        /// </returns>
        [HttpPost("StartWatchingWithCallerNameFromAttribute")]
        [MethodCaller("testCaller")]
        public ActionResult<string> StartWatchingWithCallerNameFromAttribute([FromBody] string value)
        {
            // the method’s performance info will be amended with the caller's name (if internal HttpContextAccessor is null)
            using var pm = PerformanceMeter<PerformanceMeterController>.StartWatching();
            return Ok(value);
        }

        #endregion

        #region WithDiagnosticSource

        /// <summary>
        /// Test GET method watching with diagnostic source.
        /// </summary>
        /// <param name="s">Input string.</param>
        [HttpGet("StartWatchingWithDiagnosticSource")]
        [MethodCaller("dsCaller")]
        [MethodCustomData("customData!", "diagnosticSource")]
        [WatchingWithDiagnosticSource]
        public ActionResult StartWatchingWithDiagnosticSource(string s)
        {
            return Ok();
        }

        /// <summary>
        /// Test DTO for POST method.
        /// </summary>
        public class DTOArgument
        {
            /// <summary>
            /// Test Data.
            /// </summary>
            public string Data { get; set; }
        }

        /// <summary>
        /// Test POST method watching with diagnostic source (add arguments to custom data).
        /// </summary>
        /// <param name="arg">Input DTO.</param>
        [HttpPost("StartWatchingWithDiagnosticSourceAndArguments")]
        [MethodCaller("dsCaller2")]
        [MethodCustomData("customData!", "diagnosticSourcePOST")]
        [WatchingWithDiagnosticSource]
        [AddMethodArgumentsToCustomData]
        public ActionResult StartWatchingWithDiagnosticSourceAndArguments(DTOArgument arg)
        {
            return Ok();
        }

        #endregion

        #region WithWatchingPerformanceAttribute

        /// <summary>
        /// Test GET method with WatchingPerformance attribute.
        /// </summary>
        [HttpGet("SimpleWatchingMethodStartWatchingPerformanceAttribute")]
        [WatchingPerformance]
        public ActionResult SimpleWatchingMethodStartWatchingPerformanceAttribute()
        {
            //put your code with some logic here
            Thread.Sleep(1000);

            return Ok();
        }

        #endregion

        #endregion
    }
}
