using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Unchase.PerformanceMeter
{
    /// <summary>
    /// Класс для запуска и остановки замера производительности метода.
    /// </summary>
    /// <typeparam name="TClass">Класс с методами.</typeparam>
    public sealed class PerformanceMeter<TClass> : IDisposable where TClass : class
    {
        #region Fields

        private IHttpContextAccessor _httpContextAccessor;

        private readonly MethodInfo _method;

        /// <summary>
        /// Сведения о методе.
        /// </summary>
        /// <returns>
        /// Возвращает сведения о методе типа <see cref="MethodInfo"/>.
        /// </returns>
        public MethodInfo MethodInfo => _method;

        private Stopwatch _sw = new Stopwatch();

        private DateTime _dateStart = DateTime.Now;

        private string _caller = "unknown";

        private Action<Exception> _exceptionHandler { get; set; }

        private static readonly object PerformanceMeterLock = new object();

        private static Action<Exception> _defaultExceptionHandler { get; set; }
        private static Action<Exception> DefaultExceptionHandler
        {
            get
            {
                lock (PerformanceMeterLock)
                {
                    return _defaultExceptionHandler;
                }
            }
            set
            {
                lock (PerformanceMeterLock)
                {
                    _defaultExceptionHandler = value;
                }
            }
        }

        private static ConcurrentDictionary<string, MethodInfo> _cachedMethodInfos = new ConcurrentDictionary<string, MethodInfo>();

        #endregion

        #region Constructors

        /// <summary>
        /// Приватный конструктор класса <see cref="PerformanceMeter{TClass}"/>.
        /// </summary>
        private PerformanceMeter(MethodInfo method)
        {
            _method = method;
            _exceptionHandler = DefaultExceptionHandler;
        }

        /// <summary>
        /// Статический конструктор класса <see cref="PerformanceMeter{TClass}"/>.
        /// </summary>
        static PerformanceMeter() { }

        #endregion

        #region Methods

        #region Watching

        /// <summary>
        /// Создать экземпляр класса для замера производительности метода.
        /// </summary>
        /// <param name="method">Метод типа <see cref="MethodInfo"/>.</param>
        /// <returns>
        /// Возвращает экземпляр класса <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> Watching(MethodInfo method)
        {
            if (!_cachedMethodInfos.Contains(new KeyValuePair<string, MethodInfo>(method.Name, method)))
                _cachedMethodInfos.TryAdd(method.Name, method);

            return new PerformanceMeter<TClass>(method);
        }

        /// <summary>
        /// Создать экземпляр класса для замера производительности метода.
        /// </summary>
        /// <param name="methodName">Имя метода.</param>
        /// <returns>
        /// Возвращает экземпляр класса <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> Watching(string methodName)
        {
            MethodInfo methodInfo;
            if (_cachedMethodInfos.ContainsKey(methodName))
                methodInfo = _cachedMethodInfos[methodName];
            else
            {
                methodInfo = typeof(TClass)
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                    .FirstOrDefault(m => m.Name == methodName);
                _cachedMethodInfos.TryAdd(methodName, methodInfo);
            }

            return new PerformanceMeter<TClass>(methodInfo);
        }

        #endregion

        #region Set additional fields

        /// <summary>
        /// Установить <see cref="IHttpContextAccessor"/> для получения ip адреса вызывающего метод.
        /// </summary>
        /// <param name="httpContextAccessor"><see cref="IHttpContextAccessor"/>.</param>
        internal void SetHttpContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Установить Action для обработки возникающих исключений.
        /// </summary>
        /// <param name="exceptionHandler">Action для обработки возникающих исключений.</param>
        internal void SetExceptionHandler(Action<Exception> exceptionHandler = null)
        {
            _exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Установить вызывающего клиента.
        /// </summary>
        /// <param name="caller">Вызывающий клиент.</param>
        internal void SetCallerAddress(string caller)
        {
            _caller = caller;
        }

        /// <summary>
        /// Установить свой обработчик получения данных о производительности методов.
        /// </summary>
        /// <param name="performanceInfo"><see cref="IPerformanceInfo"/>.</param>
        public static void SetCustomPerformanceInfo(IPerformanceInfo performanceInfo)
        {
            Performance<TClass>.SetCustomPerformanceInfo(performanceInfo);
        }

        /// <summary>
        /// Установить Action по-умолчанию для обработки возникающих исключений.
        /// </summary>
        /// <param name="exceptionHandler">Action для обработки возникающих исключений.</param>
        public static void SetDefaultExceptionHandler(Action<Exception> exceptionHandler = null)
        {
            DefaultExceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Установить время хранения данных о вызовах.
        /// </summary>
        /// <param name="minutes">Время хранения данных о вызовах методов в минутах.</param>
        public static void SetMethodCallsCacheTime(int minutes)
        {
            Performance<TClass>.MethodCallsCacheTime = minutes;
        }

        #endregion

        #region Main

        /// <summary>
        /// Запустить замер производительности метода.
        /// </summary>
        internal void Start()
        {
            _dateStart = DateTime.Now;
            _sw = Stopwatch.StartNew();
            Performance<TClass>.Input(_method, _exceptionHandler);
        }

        /// <summary>
        /// Получить сведения о производительности методов.
        /// </summary>
        /// <returns>
        /// Возвращает сведения о производительности методов типа <see cref="PerformanceInfo{TClass}"/>.
        /// </returns>
        public static IPerformanceInfo GetPerformanceInfo() => Performance<TClass>.PerformanceInfo;

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            _caller = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? _caller;
            Performance<TClass>.Output(_caller, _method, _sw, _dateStart, _exceptionHandler);
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Методы расширения для класса <see cref="PerformanceMeter{TClass}"/>
    /// </summary>
    public static class PerformanceMeterExtensions
    {
        #region Extension methods

        /// <summary>
        /// Установить <see cref="IHttpContextAccessor"/> для получения ip адреса вызывающего метод.
        /// </summary>
        /// <typeparam name="TClass">Класс с методами.</typeparam>
        /// <param name="performanceMeter"><see cref="PerformanceMeter{TClass}"/>.</param>
        /// <param name="httpContextAccessor"><see cref="IHttpContextAccessor"/>.</param>
        /// <returns>
        /// Возвращает <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> WithHttpContextAccessor<TClass>(this PerformanceMeter<TClass> performanceMeter, IHttpContextAccessor httpContextAccessor) where TClass : class
        {
            performanceMeter.SetHttpContextAccessor(httpContextAccessor);
            return performanceMeter;
        }

        /// <summary>
        /// Установить Action для обработки возникающих исключений.
        /// </summary>
        /// <typeparam name="TClass">Класс с методами.</typeparam>
        /// <param name="performanceMeter"><see cref="PerformanceMeter{TClass}"/>.</param>
        /// <param name="exceptionHandler">Action для обработки возникающих исключений.</param>
        /// <returns>
        /// Возвращает <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> WithExceptionHandler<TClass>(this PerformanceMeter<TClass> performanceMeter, Action<Exception> exceptionHandler = null) where TClass : class
        {
            performanceMeter.SetExceptionHandler(exceptionHandler);
            return performanceMeter;
        }

        /// <summary>
        /// Установить вызывающего клиента.
        /// </summary>
        /// <typeparam name="TClass">Класс с методами.</typeparam>
        /// <param name="performanceMeter"><see cref="PerformanceMeter{TClass}"/>.</param>
        /// <param name="caller">Вызывающий клиент.</param>
        /// <returns>
        /// Возвращает <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> WithCaller<TClass>(this PerformanceMeter<TClass> performanceMeter, string caller) where TClass : class
        {
            performanceMeter.SetCallerAddress(caller);
            return performanceMeter;
        }

        /// <summary>
        /// Запустить замер производительности метода.
        /// </summary>
        /// <typeparam name="TClass">Класс с методами.</typeparam>
        /// <param name="performanceMeter"><see cref="PerformanceMeter{TClass}"/>.</param>
        /// <returns>
        /// Возвращает <see cref="PerformanceMeter{TClass}"/>.
        /// </returns>
        public static PerformanceMeter<TClass> Start<TClass>(this PerformanceMeter<TClass> performanceMeter) where TClass : class
        {
            performanceMeter.Start();
            return performanceMeter;
        }

        #endregion
    }
}
