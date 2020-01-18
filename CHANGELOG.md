# Road map

- [ ] Fix open [issues](https://github.com/unchase/Unchase.FluentPerformanceMeter/issues/)
- [ ] Gather [feedback](https://github.com/unchase/Unchase.FluentPerformanceMeter/issues/new) for plan further features to implement

# Change log

These are the changes to each version that has been released on the official [NuGet Gallery (Common)](https://www.nuget.org/packages/Unchase.FluentPerformanceMeter) and [NuGet Gallery (MVC)](https://www.nuget.org/packages/Unchase.FluentPerformanceMeter.AspNetCore.Mvc).

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