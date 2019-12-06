#
![Unchase Fluent Performance Meter Logo](img/logo.png)


----------

[Russian Documentation](README_RU.md) | [English Documentation](README.md)


----------


**Unchase Fluent Performance Meter** is an open-source and cross-platform *.Net Standart 2.0* library is designed for watching the performance of methods.

The library can be used in .NET Core and .NET Framework applications that support *.Net Standart 2.0*, and allows:

* [**Make exact measurements**](#SimpleSamples) of the performance of ***public* methods** for ***public* classes** for your code and [used libraries code](#SampleExternal) (with fixing the exact time of the start and end of the measurement);

* [**Add Custom Data**](#SampleCustomData) to the measurement results. For example, the values of the input parameters of the method and the result; or method execution context data; or *corellationId*, by which it will be possible to link several measurements of the performance of methods;

* [**Split**](#SampleCustomData) method performance measurement **into separate steps** with fixing of own data for each step. In addition, you can [set the minimum execution time](#SampleIgnore), starting from which the step will be save into the measurement (if the step is completed faster, it will not be saved);

* [**Exclude individual parts of the code**](#SampleIgnore) from performance measurement (for example, calls to individual methods whose execution time does not need to be watching);

* [**Add custom Commands**](#SampleCustomCommands), which are guaranteed **to be executed immediately after the end of the measurement** of the method’s performance (for example, to add additional processing of the obtained results, such as logging or writing data to the storage);

* [**Add custom Exception Handler**](#SampleCustomExceptionHandler) for code executed in the context of measuring the performance of the method (for all measurements, and for each measurement separately);

* [**Set the Cache Time**](#SampleSetCacheTime) for the results of measurements of the methods performance, after which the results will be deleted;

* [**Add to the measurement results**](#SampleSetCallerAndSourceWithStop) data about, **who is calling** (Caller) the method  with *IHttpContextAccesor* or setting Caller in the code (for example, you can specify the name of the external service that called the method);

* [**Add to the measurement results**](#SampleSetCallerAndSourceWithStop) data on the **place** where the performance measurement was started (file name and line number with the place of the call in the code);

* [**Stop watching**](#SampleSetCallerAndSourceWithStop) method performance **before the end of its execution**.

The data obtained as a result of measuring the methods performance can be used to analyze the performance of the application (its individual parts, and internal — native code, or external — the code of used libraries) and displayed in a graphical form convenient for you. For example:

![Performance charts](img/charts1.png)

![Performance charts](img/charts2.png)

![Performance charts](img/charts3.png)

![Performance charts](img/charts4.png)

> The project is developed and maintained by [Nikolay Chebotov (**Unchase**)](https://github.com/unchase).

## Builds status

|Status|Value|
|:----|:---:|
|Build|[![Build status](https://ci.appveyor.com/api/projects/status/5whpp549pnr3gs6n)](https://ci.appveyor.com/project/unchase/unchase.fluentperformancemeter)
|Buid History|![Build history](https://buildstats.info/appveyor/chart/unchase/unchase-fluentperformancemeter)
|GitHub Release|[![GitHub release](https://img.shields.io/github/release/unchase/Unchase.fluentperformancemeter.svg)](https://github.com/unchase/Unchase.fluentperformancemeter/releases/latest)
|GitHub Release Date|[![GitHub Release Date](https://img.shields.io/github/release-date/unchase/Unchase.fluentperformancemeter.svg)](https://github.com/unchase/Unchase.fluentperformancemeter/releases/latest)
|GitHub Release Downloads|[![Github Releases](https://img.shields.io/github/downloads/unchase/Unchase.fluentperformancemeter/total.svg)](https://github.com/unchase/Unchase.fluentperformancemeter/releases/latest)

## Table of content

* [Getting Started](#Start)
* [Examples of using](#SimpleSamples)
	* [Method Performance Measurement](#SimpleSamples)
	* [Measuring the performance of the method of the used external library](#SampleExternal)
	* [Add Custom Data and split on Steps](#SampleCustomData)
	* [Ignore watching](#SampleIgnore)
	* [Add custom Commands and Actions](#SampleCustomCommands)
	* [Add an Exception Handlers](#SampleCustomExceptionHandler)
	* [Set the Cache Time](#SampleSetCacheTime)
	* [Adding caller and source place data (and stop watching)](#SampleSetCallerAndSourceWithStop)

## <a name="Start"></a> Getting Started

To use the library, install [*NuGet* пакет](https://www.nuget.org/packages/Unchase.FluentPerformanceMeter/) in your project:

#### Manually with the *NuGet* Package Manager:

```powershell
Install-Package Unchase.FluentPerformanceMeter
```

#### Using the .NET CLI:

```powershell
dotnet add package Unchase.FluentPerformanceMeter --version {version}
```

> Where {version} is the version of the package you want to install. 
> For example, `dotnet add package Unchase.FluentPerformanceMeter --version 1.0.0`

## <a name="SimpleSamples"></a> Examples of using

### Method Performance Measurement

The following is a simple library usage example (without configuration and additional settings) to measure method performance (Action) `SimpleWatchingMethodStart` for Controller `PerformanceMeterController` in *Asp.Net Core 2.2 WebAPI* application. You can use the extension method `.WatchingMethod().Start()` or `.StartWatching()` for this.

> All examples of using the library can be found in the `Unchase.FluentPerformanceMeter.Test*` projects of this repository.

```csharp
/// <summary>
/// Test GET method with simple performance watching.
/// </summary>
[HttpGet("SimpleWatchingMethodStart")]
public ActionResult SimpleWatchingMethodStart()
{	
    // for C# 8 you can use:
    //using var pm = PerformanceMeter<PerformanceMeterController>.StartWatching();

    using (PerformanceMeter<PerformanceMeterController>.WatchingMethod().Start())
    {
        // put your code with some logic there

        return Ok();
    }
}
```

To get the results of performance measurements of public methods of the controller class `PerformanceMeterController` you can call the following method:

```csharp
/// <summary>
/// Get methods performance info for this controller.
/// </summary>
/// <returns>Returns methods performance info.</returns>
[HttpGet("GetPerformanceInfo")]
[IgnoreMethodPerformance]
public ActionResult<IPerformanceInfo> GetPerformanceInfo()
{
    return Ok(PerformanceMeter<PerformanceMeterController>.PerformanceInfo);
}
```

After calling the method `SimpleWatchingMethodStart` and after calling `GetPerformanceInfo` we get:

```json
{
  "methodCalls": [
    {
      "methodName": "SimpleWatchingMethodStart",
      "elapsed": "00:00:00.0016350",
      "caller": "unknown",
      "startTime": "2019-12-06T10:27:27.3385385Z",
      "endTime": "2019-12-06T10:27:27.3401735Z",
      "customData": {},
      "steps": []
    }
  ],
  "totalActivity": [
    {
      "methodName": "SimpleWatchingMethodStart",
      "callsCount": 1
    }
  ],
  "currentActivity": [
    {
      "methodName": "SimpleWatchingMethodStart",
      "callsCount": 0
    }
  ],
  "uptimeSince": "2019-12-06T10:27:27.3370183Z",
  "className": "Unchase.FluentPerformanceMeter.TestWebAPI.Controllers.PerformanceMeterController",
  "methodNames": [
    "SimpleWatchingMethodStart"
  ],
  "customData": {},
  "timerFrequency": 10000000
}
```

### <a name="SampleExternal"></a> Measuring the performance of the method of the used external library

To measure the performance of the *public* method of the *public* class of a third-party used library, you must explicitly set the class itself and the name of its method:

```csharp
[HttpGet("GetThreadSleepPerformance")]
public ActionResult<string> GetThreadSleepPerformance()
{
    using (PerformanceMeter<Thread>.WatchingMethod(nameof(Thread.Sleep)).Start())
    {
        Thread.Sleep(1000);
    }

    return Ok(PerformanceMeter<Thread>.PerformanceInfo.MethodCalls.FirstOrDefault(ta => ta.MethodName == nameof(Thread.Sleep))?.Elapsed);
}
```

The executed method will return:

```
"00:00:01.0033040"
```

You can get performance data about calling this method through a call:

```csharp
/// <summary>
/// Get methods performance info for Thread class.
/// </summary>
/// <returns>Returns Thread methods performance info.</returns>
[HttpGet("GetThreadPerformanceInfo")]
[IgnoreMethodPerformance]
public ActionResult<IPerformanceInfo> GetThreadPerformanceInfo()
{
    return Ok(PerformanceMeter<Thread>.PerformanceInfo);
}
```

In response to a call to this method will be:

```json
{
  "methodCalls": [
    {
      "methodName": "Sleep",
      "elapsed": "00:00:01.0033040",
      "caller": "unknown",
      "startTime": "2019-12-06T13:08:09.336624Z",
      "endTime": "2019-12-06T13:08:10.339928Z",
      "customData": {},
      "steps": []
    }
  ],
  "totalActivity": [
    {
      "methodName": "Abort",
      "callsCount": 0
    },
    {
      "methodName": "Abort",
      "callsCount": 0
    },
    {
      "methodName": "ResetAbort",
      "callsCount": 0
    },
    {
      "methodName": "Suspend",
      "callsCount": 0
    },
    {
      "methodName": "Resume",
      "callsCount": 0
    },
    {
      "methodName": "BeginCriticalRegion",
      "callsCount": 0
    },
    {
      "methodName": "EndCriticalRegion",
      "callsCount": 0
    },
    {
      "methodName": "BeginThreadAffinity",
      "callsCount": 0
    },
    {
      "methodName": "EndThreadAffinity",
      "callsCount": 0
    },
    {
      "methodName": "AllocateDataSlot",
      "callsCount": 0
    },
    {
      "methodName": "AllocateNamedDataSlot",
      "callsCount": 0
    },
    {
      "methodName": "GetNamedDataSlot",
      "callsCount": 0
    },
    {
      "methodName": "FreeNamedDataSlot",
      "callsCount": 0
    },
    {
      "methodName": "GetData",
      "callsCount": 0
    },
    {
      "methodName": "SetData",
      "callsCount": 0
    },
    {
      "methodName": "SetApartmentState",
      "callsCount": 0
    },
    {
      "methodName": "TrySetApartmentState",
      "callsCount": 0
    },
    {
      "methodName": "GetCompressedStack",
      "callsCount": 0
    },
    {
      "methodName": "SetCompressedStack",
      "callsCount": 0
    },
    {
      "methodName": "GetCurrentProcessorId",
      "callsCount": 0
    },
    {
      "methodName": "GetDomain",
      "callsCount": 0
    },
    {
      "methodName": "GetDomainID",
      "callsCount": 0
    },
    {
      "methodName": "GetHashCode",
      "callsCount": 0
    },
    {
      "methodName": "Interrupt",
      "callsCount": 0
    },
    {
      "methodName": "Join",
      "callsCount": 0
    },
    {
      "methodName": "Join",
      "callsCount": 0
    },
    {
      "methodName": "Join",
      "callsCount": 0
    },
    {
      "methodName": "MemoryBarrier",
      "callsCount": 0
    },
    {
      "methodName": "Sleep",
      "callsCount": 1
    },
    {
      "methodName": "Sleep",
      "callsCount": 0
    },
    {
      "methodName": "SpinWait",
      "callsCount": 0
    },
    {
      "methodName": "Yield",
      "callsCount": 0
    },
    {
      "methodName": "Start",
      "callsCount": 0
    },
    {
      "methodName": "Start",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "GetApartmentState",
      "callsCount": 0
    },
    {
      "methodName": "DisableComObjectEagerCleanup",
      "callsCount": 0
    }
  ],
  "currentActivity": [
    {
      "methodName": "Abort",
      "callsCount": 0
    },
    {
      "methodName": "Abort",
      "callsCount": 0
    },
    {
      "methodName": "ResetAbort",
      "callsCount": 0
    },
    {
      "methodName": "Suspend",
      "callsCount": 0
    },
    {
      "methodName": "Resume",
      "callsCount": 0
    },
    {
      "methodName": "BeginCriticalRegion",
      "callsCount": 0
    },
    {
      "methodName": "EndCriticalRegion",
      "callsCount": 0
    },
    {
      "methodName": "BeginThreadAffinity",
      "callsCount": 0
    },
    {
      "methodName": "EndThreadAffinity",
      "callsCount": 0
    },
    {
      "methodName": "AllocateDataSlot",
      "callsCount": 0
    },
    {
      "methodName": "AllocateNamedDataSlot",
      "callsCount": 0
    },
    {
      "methodName": "GetNamedDataSlot",
      "callsCount": 0
    },
    {
      "methodName": "FreeNamedDataSlot",
      "callsCount": 0
    },
    {
      "methodName": "GetData",
      "callsCount": 0
    },
    {
      "methodName": "SetData",
      "callsCount": 0
    },
    {
      "methodName": "SetApartmentState",
      "callsCount": 0
    },
    {
      "methodName": "TrySetApartmentState",
      "callsCount": 0
    },
    {
      "methodName": "GetCompressedStack",
      "callsCount": 0
    },
    {
      "methodName": "SetCompressedStack",
      "callsCount": 0
    },
    {
      "methodName": "GetCurrentProcessorId",
      "callsCount": 0
    },
    {
      "methodName": "GetDomain",
      "callsCount": 0
    },
    {
      "methodName": "GetDomainID",
      "callsCount": 0
    },
    {
      "methodName": "GetHashCode",
      "callsCount": 0
    },
    {
      "methodName": "Interrupt",
      "callsCount": 0
    },
    {
      "methodName": "Join",
      "callsCount": 0
    },
    {
      "methodName": "Join",
      "callsCount": 0
    },
    {
      "methodName": "Join",
      "callsCount": 0
    },
    {
      "methodName": "MemoryBarrier",
      "callsCount": 0
    },
    {
      "methodName": "Sleep",
      "callsCount": 0
    },
    {
      "methodName": "Sleep",
      "callsCount": 0
    },
    {
      "methodName": "SpinWait",
      "callsCount": 0
    },
    {
      "methodName": "Yield",
      "callsCount": 0
    },
    {
      "methodName": "Start",
      "callsCount": 0
    },
    {
      "methodName": "Start",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileRead",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "VolatileWrite",
      "callsCount": 0
    },
    {
      "methodName": "GetApartmentState",
      "callsCount": 0
    },
    {
      "methodName": "DisableComObjectEagerCleanup",
      "callsCount": 0
    }
  ],
  "uptimeSince": "2019-12-06T13:08:09.3357028Z",
  "className": "System.Threading.Thread",
  "methodNames": [
    "Abort",
    "ResetAbort",
    "Suspend",
    "Resume",
    "BeginCriticalRegion",
    "EndCriticalRegion",
    "BeginThreadAffinity",
    "EndThreadAffinity",
    "AllocateDataSlot",
    "AllocateNamedDataSlot",
    "GetNamedDataSlot",
    "FreeNamedDataSlot",
    "GetData",
    "SetData",
    "SetApartmentState",
    "TrySetApartmentState",
    "GetCompressedStack",
    "SetCompressedStack",
    "GetCurrentProcessorId",
    "GetDomain",
    "GetDomainID",
    "GetHashCode",
    "Interrupt",
    "Join",
    "MemoryBarrier",
    "Sleep",
    "SpinWait",
    "Yield",
    "Start",
    "VolatileRead",
    "VolatileWrite",
    "GetApartmentState",
    "DisableComObjectEagerCleanup"
  ],
  "customData": {},
  "timerFrequency": 10000000
}
```

### <a name="SampleCustomData"></a> Add Custom Data and split on Steps

You can add Custom Data for all performance measurements of methods of a particular class. For example, in the static constructor of the `PerformanceMeterController` controller class:

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class PerformanceMeterController : ControllerBase
{
    /// <summary>
    /// Static constructor.
    /// </summary>
    static PerformanceMeterController()
    {
        // add common custom data (string) to class performance information
        PerformanceMeter<PerformanceMeterController>.AddCustomData("Tag", "CustomTag");

        // add common custom data (anonymous class) to class performance information
        PerformanceMeter<PerformanceMeterController>.AddCustomData("Custom anonymous class", new { Name = "Custom Name", Value = 1 });
    }

    // ... actions and others
}
```

In addition, you can add Custom Data for a specific measurement using the extension method `.WithSettingData.CustomData("<key>", <value>)` (including through the special attribute `MethodCustomDataAttribute`) and for each Step of this measurement, added using the extension method `.Step("<step_name>")`:

```csharp
/// <summary>
/// Test GET method with simple performance watching (with steps).
/// </summary>
[HttpGet("SimpleStartWatchingWithSteps")]
[MethodCustomData("Custom data from attribute", "Attr")]
public ActionResult SimpleStartWatchingWithSteps()
{
    using (var pm = PerformanceMeter<PerformanceMeterController>
        .WatchingMethod()
        .WithSettingData
            .CustomData("coins", 1)
            .CustomData("Coins sets", new 
            { 
                Gold = "Many",
                Silver = 5
            })
        .Start())
    {
        // put your code with some logic there

        // add "Step 1"
        using (pm.Step("Step 1"))
        {
            Thread.Sleep(1000);
        }

        // add "Step 2" with custom data
        using (var pmStep = pm.Step("Step 2").AddCustomData("step2 custom data", "data!"))
        {
            // add "Step 3 in Step 2"
            using (pm.Step("Step 3 in Step 2"))
            {
                Thread.Sleep(1000);
            }

            // add custom data to "Step 2"
            pmStep.AddCustomData("step2 another custom data", "data2!");

            // get and remove custom data from "Step 2"
            var customData = pmStep.GetAndRemoveCustomData<string>("step2 custom data");
        
            // ...
        }
    }
}
```

As a result, when called `GetPerformanceInfo` we get:

```json
{
  "methodCalls": [
    {
      "methodName": "SimpleStartWatchingWithSteps",
      "elapsed": "00:00:02.0083031",
      "caller": "unknown",
      "startTime": "2019-12-06T11:58:18.9006891Z",
      "endTime": "2019-12-06T11:58:20.9089922Z",
      "customData": {
        "Coins sets": {
          "gold": "Many",
          "silver": 5
        },
        "coins": 1,
        "Custom data from attribute": "Attr"
      },
      "steps": [
        {
          "stepName": "Step 1",
          "elapsed": "00:00:01.0009758",
          "startTime": "2019-12-06T11:58:18.9018272Z",
          "endTime": "2019-12-06T11:58:19.902803Z",
          "customData": {}
        },
        {
          "stepName": "Step 3 in Step 2",
          "elapsed": "00:00:01.0004549",
          "startTime": "2019-12-06T11:58:19.9046523Z",
          "endTime": "2019-12-06T11:58:20.9051072Z",
          "customData": {}
        },
        {
          "stepName": "Step 2",
          "elapsed": "00:00:01.0029596",
          "startTime": "2019-12-06T11:58:19.904534Z",
          "endTime": "2019-12-06T11:58:20.9074936Z",
          "customData": {
            "step2 another custom data": "data2!"
          }
        }
      ]
    }
  ],
  "totalActivity": [
    {
      "methodName": "SimpleStartWatchingWithSteps",
      "callsCount": 1
    }
  ],
  "currentActivity": [
    {
      "methodName": "SimpleStartWatchingWithSteps",
      "callsCount": 0
    }
  ],
  "uptimeSince": "2019-12-06T11:58:18.8801249Z",
  "className": "Unchase.FluentPerformanceMeter.TestWebAPI.Controllers.PerformanceMeterController",
  "methodNames": [
    "SimpleStartWatchingWithSteps"
  ],
  "customData": {
    "Tag": "CustomTag",
    "Custom anonymous class": {
      "name": "Custom Name",
      "value": 1
    }
  },
  "timerFrequency": 10000000
}
```

### <a name="SampleIgnore"></a> Ignore watching

You can ignore individual parts of the method in measuring performance (using `.Ignore()` or `.Executing().WithoutWatching().Start(<Action>)` extension methods), and also do not save individual Steps (with `.StepIf("<step_name>", <minSaveMs>)` extension method), if they do not satisfy the condition (the step execution time will be taken into the method execution time):

```csharp
using (PerformanceMeter<PerformanceMeterController>.WatchingMethod().Start())
{
    // put your code with some logic there

    // sleep 1 sec
    Thread.Sleep(1000);

	// ignore this block in performance watching
    using (pm.Ignore())
    {
        Thread.Sleep(5000);
    }

    // skip this step with minSaveMs (not save, but consider duration in method performance watching)
    using (pm.StepIf("Skipped step", minSaveMs: 1000))
    {
        Thread.Sleep(500);
    }

    // execute action without performance watching
    pm.Executing().WithoutWatching().Start(() => 
    {
        Thread.Sleep(2000);
    });

    return Ok();
}
```

As a result, we get:

```json
{
  "methodCalls": [
    {
      "methodName": "SimpleStartWatchingWithIgnored",
      "elapsed": "00:00:01.5080227",
      "caller": "unknown",
      "startTime": "2019-12-06T12:34:36.9187359Z",
      "endTime": "2019-12-06T12:34:38.4267586Z",
      "customData": {},
      "steps": []
    }
  ],
  "totalActivity": [
    {
      "methodName": "SimpleStartWatchingWithIgnored",
      "callsCount": 1
    }
  ],
  "currentActivity": [
    {
      "methodName": "SimpleStartWatchingWithIgnored",
      "callsCount": 0
    }
  ],
  "uptimeSince": "2019-12-06T12:34:36.9035129Z",
  "className": "Unchase.FluentPerformanceMeter.TestWebAPI.Controllers.PerformanceMeterController",
  "methodNames": [
    "SimpleStartWatchingWithIgnored"
  ],
  "customData": { },
  "timerFrequency": 10000000
}
```

### <a name="SampleCustomCommands"></a> Add custom Commands and Actions

To add a custom Command that will be guaranteed to be executed upon completion of measuring the performance of a method, it is necessary to create a command class that will implement the `IPerformanceCommand` interface. 
In this case, you can transfer arbitrary data through the constructor of the created command that will be used when the command is executed, for example:

```csharp
/// <summary>
/// Custom executed command.
/// </summary>
public class ExecutedCommand : IPerformanceCommand
{
    /// <summary>
    /// Executed commad name.
    /// </summary>
    public string CommandName => this.GetType().Name;

    private string _customString { get; }

    internal bool IsCommandExecuted { get; private set; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <remarks>
    /// You can pass any data through the command constructor.
    /// </remarks>
    /// <param name="customString"></param>
    public ExecutedCommand(string customString) 
    {
        this._customString = customString;
    }

    /// <summary>
    /// Execute command.
    /// </summary>
    /// <param name="performanceInfo"><see cref="IPerformanceInfo"/>.</param>
    public void Execute(IPerformanceInfo performanceInfo)
    {
        // for example, write to the debug console some information
        Debug.WriteLine(this.CommandName);
        Debug.WriteLine(this._customString);
        Debug.WriteLine($"Method names count: {performanceInfo.MethodNames.Count}");
        this.IsCommandExecuted = true;
    }
}
```

You can add a custom Command and an Action so that they are executed at the end of the measurement in the following way:

```csharp
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
```

As a result, at the end of measuring the performance of the method in the Debug console, it will display:

```
ExecutedCommand
bla-bla-bla
Method names count: 13
Class name: Unchase.FluentPerformanceMeter.TestWebAPI.Controllers.PerformanceMeterController
```

### <a name="SampleCustomExceptionHandler"></a> Add an Exception Handlers

If you need to handle exceptions that may occur during the execution of a part of the method for which performance is watched, you need to add an Exception Handler as follows:

```csharp
using (var pm = PerformanceMeter<PerformanceMeterController>.StartWatching())
{
    // execute action throws Exception with exception handler
    pm.Executing()
        .WithExceptionHandler((ex) => Debug.WriteLine(ex.Message))
        .Start(() => throw new Exception("Exception"));

    // execute action throws custom Exception with exception handler
    pm.Executing<CustomException>()
       .WithExceptionHandler((ex) => { Debug.WriteLine(ex.Message); })
       .Start(() =>
       {
           throw new CustomException("Custom exception was occured!");
       });

    return Ok();
}
```

Where the `CustomException` class is:

```csharp
/// <summary>
/// Custom exception.
/// </summary>
public class CustomException : Exception
{
    public CustomException(string message) : base(message) { }

    public CustomException(string message, Exception innerException) : base(message, innerException) { }

    public CustomException() { }
}
```

As a result, the following will be displayed in the Debug console:

```
Exception
Custom exception was occured!
```

In addition, you can specify an Exception Handler that will be used by default to measure the performance of any method of this class, for example, through the static constructor of the `PerformanceMeterController` controller class:

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class PerformanceMeterController : ControllerBase
{
    /// <summary>
    /// Static constructor.
    /// </summary>
    static PerformanceMeterController()
    {
        // set default exception handler for PerformanceMeterController class
        PerformanceMeter<PerformanceMeterController>.SetDefaultExceptionHandler((ex) => Debug.WriteLine(ex.Message));
    }

    // ... actions and others
}
```

### <a name="#SampleSetCacheTime"></a> Set the Cache Time

You can set the Cache Time for the data of the performance measurements of methods, after which this data will be deleted. For each class for which measurement is made, this time is set separately. For example, the time can be set through the static constructor of the `PerformanceMeterController` controller class:

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class PerformanceMeterController : ControllerBase
{
    /// <summary>
    /// Static constructor.
    /// </summary>
    static PerformanceMeterController()
    {
        // set cache time for PerformanceMeterController class
        PerformanceMeter<PerformanceMeterController>.SetMethodCallsCacheTime(5);
    }

    // ... actions and others
}
```

### <a name="SampleSetCallerAndSourceWithStop"></a> Adding caller and source place data (and stop watching)

You can specify who is calling the method using the extension method `.CallerFrom("<caller_name>")` (either a string or *IHttpContextAccessor* is passed to it) or a special attribute `[MethodCaller ("<caller_name>")]` for the method. Moreover, if both the attribute and the extension method are used, then the value will be taken from the latter.

To add a call source for measuring performance, use the extension method `.CallerSourceData()`.

To stop measuring performance inside the *using* block, use the `.StopWatching()` extension method or the `Dispose()` method directly:

```csharp
[HttpPost("StartWatchingWithCallerName")]
[MethodCaller("testCaller")]
public ActionResult<string> StartWatchingWithCallerName([FromBody] string value)
{
    // method performance info will reach with caller name (if internal HttpContextAccessor is null)
    using (var pm = PerformanceMeter<PerformanceMeterController>
        .WatchingMethod()
        .WithSettingData
            .CallerSourceData()
            .CallerFrom("Test caller")
        .Start())
    {
        pm.StopWatching(); // stop watching there (or you can use "pm.Dispose();")
        Thread.Sleep(2000);

        return Ok(value);
    }
}
```

As a result of calling the `GetPerformanceInfo` method, you will get:

```json
{
  "methodCalls": [
    {
      "methodName": "StartWatchingWithCallerName",
      "elapsed": "00:00:00.0019172",
      "caller": "Test caller",
      "startTime": "2019-12-06T13:35:45.3164507Z",
      "endTime": "2019-12-06T13:35:45.3183679Z",
      "customData": {
        "customData123": 123,
        "callerSourceLineNumber": 525,
        "callerSource": "D:\\GitHub\\Unchase.FluentPerformanceMeter\\Unchase.FluentPerformanceMeter.TestWebAPI\\Controllers\\PerformanceMeterController.cs"
      },
      "steps": []
    }
  ],
  "totalActivity": [
    {
      "methodName": "StartWatchingWithCallerName",
      "callsCount": 1
    }
  ],
  "currentActivity": [
    {
      "methodName": "StartWatchingWithCallerName",
      "callsCount": 0
    }
  ],
  "uptimeSince": "2019-12-06T13:35:45.2601668Z",
  "className": "Unchase.FluentPerformanceMeter.TestWebAPI.Controllers.PerformanceMeterController",
  "methodNames": [
    "StartWatchingWithCallerName"
  ],
  "customData": { },
  "timerFrequency": 10000000
}
```

## HowTos

- [ ] Add HowTos in a future
- [ ] ... [request for HowTo you need](https://github.com/unchase/Unchase.FluentPerformanceMeter/issues/new?title=DOC)

## Roadmap

See the [changelog](CHANGELOG.md) for the further development plans and version history.

## Thank me!

If you like what I am doing and you would like to thank me, please consider:

[![Buy me a coffe!](img/buymeacoffe.png)](https://www.buymeacoffee.com/nikolaychebotov)

Thank you for your support!

----------

Copyright &copy; 2019 [Nikolay Chebotov (**Unchase**)](https://github.com/unchase) - Provided under the [Apache License 2.0](LICENSE.md).

