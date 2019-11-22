using System;
using System.Diagnostics;
using System.Reflection;

namespace Unchase.PerformanceMeter
{
    /// <summary>
    /// Класс для производительности метода.
    /// </summary>
    /// <typeparam name="TClass">Класс с методами.</typeparam>
    public static class Performance<TClass> where TClass : class
    {
        #region Fields

        private static readonly object PerformanceLock = new object();

        private static DateTime _lastRemoveDate;

        #endregion

        #region Properties

        private static IPerformanceInfo _performanceInfo;
        /// <summary>
        /// Сведения о производительности метода.
        /// </summary>
        public static IPerformanceInfo PerformanceInfo
        {
            get
            {
                lock (PerformanceLock)
                {
                    if (_performanceInfo == null)
                    {
                        _performanceInfo = new PerformanceInfo<TClass>();
                        _lastRemoveDate = DateTime.Now;
                    }
                    else
                    {
                        if (_lastRemoveDate.AddMinutes(MethodCallsCacheTime) < DateTime.Now)
                        {
                            _performanceInfo.MethodCalls?.RemoveAll(x => x?.StartTime.CompareTo(DateTime.Now.AddMinutes(-MethodCallsCacheTime)) < 0);
                            _lastRemoveDate = DateTime.Now;
                        }
                    }
                    return _performanceInfo;
                }
            }
        }

        /// <summary>
        /// Время в минутах до обновления.
        /// </summary>
        /// <remarks>
        /// <see cref="IPerformanceInfo.MethodCalls"/>.
        /// </remarks>
        public static int MethodCallsCacheTime { get; set; } = 5;

        #endregion

        #region Public methods

        /// <summary>
        /// Установить свой обработчик получения данных о производительности методов.
        /// </summary>
        /// <param name="performanceInfo"><see cref="IPerformanceInfo"/>.</param>
        public static void SetCustomPerformanceInfo(IPerformanceInfo performanceInfo)
        {
            lock (PerformanceLock)
            {
                _performanceInfo = performanceInfo;
            }
        }

        /// <summary>
        /// Начать отслеживание производительности метода.
        /// </summary>
        /// <param name="method">Метод типа <see cref="MethodInfo"/>.</param>
        /// <param name="exceptionHandler">Action для обработки возникающих исключений.</param>
        public static void Input(MethodInfo method, Action<Exception> exceptionHandler = null)
        {
            try
            {
                var currentActivity = PerformanceInfo.CurrentActivity.Find(x => x.Method == method);
                if (currentActivity != null)
                    currentActivity.CallsCount++;
            }
            catch (Exception ex)
            {
                exceptionHandler(ex);
            }
        }

        /// <summary>
        /// Завершить отслеживание производительности метода.
        /// </summary>
        /// <param name="httpContextAccessor"><see cref="IHttpContextAccessor"/> для получения адреса вызывающего метод.</param>
        /// <param name="method">Метод типа <see cref="MethodInfo"/>.</param>
        /// <param name="sw"><see cref="Stopwatch"/> для отслеживания времени работы метода.</param>
        /// <param name="dateStart">Дата начала выполнения метода.</param>
        /// <param name="exceptionHandler">Action для обработки возникающих исключений.</param>
        public static void Output(string callerAddress, MethodInfo method, Stopwatch sw, DateTime dateStart, Action<Exception> exceptionHandler = null)
        {
            try
            {
                if (method != null && sw.IsRunning)
                {
                    sw.Stop();
                    var currentActivity = PerformanceInfo.CurrentActivity.Find(x => x.Method == method);
                    if (currentActivity != null)
                        currentActivity.CallsCount--;
                    PerformanceInfo.MethodCalls.Add(new MethodCallInfo<MethodInfo>(method, sw.ElapsedMilliseconds, callerAddress, dateStart, DateTime.Now));
                    var totalActivity = PerformanceInfo.TotalActivity.Find(x => x.Method == method);
                    if (totalActivity != null)
                        totalActivity.CallsCount++;
                }
            }
            catch (Exception ex)
            {
                exceptionHandler(ex);
            }
        }

        #endregion
    }
}
