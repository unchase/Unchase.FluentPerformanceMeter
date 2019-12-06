#
![Unchase Fluent Performance Meter Logo](img/logo.png)

**Unchase Fluent Performance Meter** это кросс-платформенная open-source *.Net Standart 2.0* библиотека, предназначенная для подсчёта производительности выполнения методов.

Библиотека может быть использована в .NET Core и .NET Framework приложениях, поддерживающих *.Net Standart 2.0*, и позволяет:

* [**Производить точные замеры**](#SimpleSamples) производительности ***public* методов** для ***public* классов** как вашего кода, так и [кода подключенных библиотек](#SmapleExternal) (с фиксацией точного времени начала и окончания выполнения замера);
* [**Добавлять к результатам**](#SampleCustomData) замеров **дополнительные данные** (Custom Data). Например, значения входных параметров метода и полученный результат; или данные контекста выполнения метода; или *corellationId*, по которому можно будет связать несколько замеров производительности методов;
* [**Разбивать замер**](#SampleSteps) производительности метода **на отдельные шаги** (Steps) с фиксацией собственных данных для каждого шага. Кроме того можно [задать минимальное время](#SampleMinSaveMs) выполнения, начиная с которого шаг будет учитываться в замере (если шаг выполнится быстрее, то не попадёт в замер);
* [**Исключать из замера**](#SampleIgnore) производительности **отдельные части кода** (например, вызовы отдельных методов, время исполнения которых не нужно учитывать при замере);
* [**Добавлять собственные команды**](#SampleCustomCommands) (Commands), которые гарантированно **будут исполнены сразу после** окончания замера производительности метода (например, для добавления дополнительной обработки полученных результатов, таких как логирование или запись данных в хранилище);
* [**Добавлять свой обработчик исключений**](#SampleCustomExceptionHandler) для кода, выполняемого в контексте замера производительности метода (как общий для всех замеров, так и для каждого замера в отдельности);
* [**Устанавливать время хранения результатов**](#SampleSetCacheTime) замеров производительности методов, по истечении которого результаты будут очищены;
* [**Добавить в результаты замера**](#SampleSetCallerAndSource) данные о том, **кто вызывает метод** (Caller) через *IHttpContextAccesor* или задание Caller'а в коде (например, можно указать название внешнего сервиса, который вызвал метод);
* [**Добавить в результаты замера**](#SampleSetCallerAndSource) данные о месте запуска замера производительности (название файла и номер строки с местом вызова в коде).

Полученные в результате замеров производительности методов данные можно использовать для анализа производительности приложения (отдельных его частей, как внутренних - собственный код, так и внешних - код подключенных библиотек) и вывести в удобном для вас графическом виде. Например, в таком:

![Performance charts](img/charts1.png)

![Performance charts](img/charts2.png)

![Performance charts](img/charts3.png)

![Performance charts](img/charts4.png)

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
* [Конфигурарование и кастомизация](#Customization)
* [Примеры использования](#SimpleSamples)

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

## <a name="Customization"></a> Конфигурарование и кастомизация



## <a name="SimpleSamples"></a> Примеры использования 

### Измерение производительности метода

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

### <a name="SampleCustomData"></a> Добавление дополнительных данных



### <a name="SampleSteps"></a> Разбиение на шаги



### <a name="SampleIgnore"></a> Исключение из замера



### <a name="SampleCustomCommands"></a> Добавление команд



### <a name="SampleCustomExceptionHandler"></a> Добавление обработчиков исключений



### <a name="SampleCustomExceptionHandler"></a> Установка времени хранения данных



### <a name="SampleSetCallerAndSource"></a> Добавление данных о вызывающем метод и месте вызова



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

