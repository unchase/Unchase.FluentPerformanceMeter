#
![Unchase Fluent Performance Meter Logo](img/logo.png)

**Unchase Fluent Performance Meter** предназначен для подсчёта производительности выполнения public методов для public классов. Является *.Net Standart 2.0+* библиотекой, оформленной в виде [*NuGet* пакета](https://www.nuget.org/packages/Unchase.FluentPerformanceMeter/).

> Проект разработан и поддерживается [Чеботовым Николаем (**Unchase**)](https://github.com/unchase).

## Builds status

|Status|Value|
|:----|:---:|
|Build|[![Build status](https://ci.appveyor.com/api/projects/status/5whpp549pnr3gs6n)](https://ci.appveyor.com/project/unchase/unchase.fluentperformancemeter)
|Buid History|![Build history](https://buildstats.info/appveyor/chart/unchase/unchase-fluentperformancemeter)
|GitHub Release|[![GitHub release](https://img.shields.io/github/release/unchase/Unchase.fluentperformancemeter.svg)](https://github.com/unchase/Unchase.fluentperformancemeter/releases/latest)
|GitHub Release Date|[![GitHub Release Date](https://img.shields.io/github/release-date/unchase/Unchase.fluentperformancemeter.svg)](https://github.com/unchase/Unchase.fluentperformancemeter/releases/latest)
|GitHub Release Downloads|[![Github Releases](https://img.shields.io/github/downloads/unchase/Unchase.fluentperformancemeter/total.svg)](https://github.com/unchase/Unchase.fluentperformancemeter/releases/latest)

## Содержание

* [Начало работы](#Start)
* [Пример использования](#Sample)

## <a name="Start"></a> Начало работы

Для использования библиотеки установите [*NuGet* пакет](https://www.nuget.org/packages/Unchase.FluentPerformanceMeter/) в ваш проект:

#### Вручную с помощью менеджера *NuGet* пакетов (Package Manager):

```powershell
Install-Package Unchase.FluentPerformanceMeter
```

#### С помощью .NET CLI:

```powershell
dotnet add package Unchase.FluentPerformanceMeter --version {version}
```

> Где {version} - это версия пакета, которую вы хотите установить. 
> Например, `dotnet add package Unchase.FluentPerformanceMeter --version 1.0.0`

## <a name="Sample"></a> Пример использования 

Далее приведён простейший пример использования библиотеки (без конфигурирования и дополнительных настроек) для замера производительности метода (Action) `SimpleWatchingMethodStart` контроллера (Controller) `PerformanceMeterController` *Asp.Net Core 2.2 WebAPI* приложения.

> Все примеры использования библиотеки можно найти в проектах `Unchase.FluentPerformanceMeter.Test*` данного репозитория.

```csharp
/// <summary>
/// Test GET method with simple performance watching.
/// </summary>
[HttpGet("SimpleWatchingMethodStart")]
public ActionResult SimpleWatchingMethodStart()
{
    using (PerformanceMeter<PerformanceMeterController>.WatchingMethod().Start())
    {
        // put your code with some logic there

        return Ok();
    }
}
```

Для получения результатов замеров производительности публичных методов класса-контроллера `PerformanceMeterController` можно вызвать следующий метод:

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

После вызова метода `SimpleWatchingMethodStart` при вызове `GetPerformanceInfo` получим:

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




## HowTos

- [ ] Добавить больше HowTos в будущем
- [ ] ... [запросить HowTo, которое вам необходимо](https://github.com/unchase/Unchase.FluentPerformanceMeter/issues/new?title=DOC)

## Дорожная карта (Roadmap)

Дальнейшие планы развития библиотеки и историю версий смотрите в [changelog](CHANGELOG.md).

## Поблагодарить меня

Если вам нравится то, что я делаю, и вы хотели бы поблагодарить меня, пожалуйста, поддержите по ссылке:

[![Buy me a coffe!](img/buymeacoffe.png)](https://www.buymeacoffee.com/nikolaychebotov)

Спасибо за вашу поддержку!

----------

Copyright &copy; 2019 [Nikolay Chebotov (**Unchase**)](https://github.com/unchase) - Provided under the [Apache License 2.0](LICENSE.md).

