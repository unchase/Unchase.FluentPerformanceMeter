# Road map

- [ ] Fix open [issues](https://github.com/unchase/Unchase.FluentPerformanceMeter/issues/)
- [ ] Gather [feedback](https://github.com/unchase/Unchase.FluentPerformanceMeter/issues/new) for plan further features to implement

# Change log

These are the changes to each version that has been released on the official [NuGet Gallery (Common)](https://www.nuget.org/packages/Unchase.FluentPerformanceMeter) and [NuGet Gallery (MVC)](https://www.nuget.org/packages/Unchase.FluentPerformanceMeter.AspNetCore.Mvc).

## v2.1.1 `(2020-05-02)`

- [x] Add feature: allows to iterate executing Actions with `iterations` parameter in `Start` extension methods
- [x] Update nuget-dependencies

## v2.1.0 `(2020-03-14)`

- [x] Add feature: allows to to get the performance measurements results using the built-in **DI**

## v2.0.0 `(2020-03-08)`

- [x] Add feature: possible to measure the performance of methods (actions) in an *AspNetCore MVC* application using the special `WatchingPerformanceAttribute` attribute, as well as configure the methods performance watching for the controllers in `Startup.cs`
- [x] Add code refactoring

## v1.2.3 `(2020-03-03)`

- [x] Replace all `Task` by `ValueTask`
- [x] Add `Unchase.FluentPerformanceMeter.TestWebAPI31` (.NET Core 3.1 WebApi example project)
- [x] Add `Interlocked.CompareExchange` for incrementing and decrementing calls counter

## v1.2.2 `(2020-02-20)`

- [x] Add xml-comments to nuget packages

## v1.2.1 `(2020-01-28)`

- [x] Add `HandleException` to `PerformanceDiagnosticObserver` class
- [x] Add `GetExceptionHandler` and `GetDefaultExceptionHandler` methods to `PerformanceMeter` class

**BREAKING CHANGES:**

- [x] Rename `PerformanceClassDiagnosticObserver` to `PerformanceDiagnosticObserver`
- [x] Should use `services.AddPerformanceDiagnosticObserver<MeasurableClass>();` instead of `services.AddPerformanceDiagnosticObserver<PerformanceClassDiagnosticObserver<MeasurableClass>>();` in `ConfigureServices` method in `Startup.cs`

## v1.2.0 `(2020-01-18)`

- [x] Add `TryAddCustomData` method into `PerformanceMeter` class
- [x] Add `AddMethodArgumentsToCustomDataAttribute` attribute for adding arguments to measurement custom data with `DiagnosticSource`

## v1.1.2 `(2020-01-11)`

- [x] Add `GetCustomData` method into `PerformanceMeter` and `PerformanceInfoStep` classes 

## v1.1.0 `(2020-01-06)`

 [x] Add [`Unchase.FluentPerformanceMeter.AspNetCore.Mvc`](https://www.nuget.org/Unchase.FluentPerformanceMeter.AspNetCore.Mvc) project that allows to observe method's performance measurement with `DiagnosticSource` in *AspNetCore* project

## v1.0.5 `(2019-12-30)`

- [x] Add more method overloads (with Action and Func parameter) for `PerformanceMeter<TClass>.WatchingMethod` and extension method `PerformanceMeter<TClass>.StartWatching`

## v1.0.0 `(2019-12-11)`

- [x] Release version of `Unchase Fluent Performance Meter` *NuGet* package on the official [NuGet Gallery](https://www.nuget.org/Unchase.FluentPerformanceMeter)