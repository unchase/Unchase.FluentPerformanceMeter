using System;
using System.Collections.Generic;
using System.Reflection;

namespace Unchase.PerformanceMeter
{
    /// <summary>
    /// Сведения о производительности метода.
    /// </summary>
    public interface IPerformanceInfo
    {
        /// <summary>
        /// Список вызовов метода.
        /// </summary>
        /// <remarks>
        /// <see cref="MethodCallInfo{MethodInfo}"/>.
        /// </remarks>
        List<MethodCallInfo<MethodInfo>> MethodCalls { get; set; }

        /// <summary>
        /// Список общего количества вызовов метода.
        /// </summary>
        /// <remarks>
        /// <see cref="MethodCallsCount{MethodInfo}"/>.
        /// </remarks>
        List<MethodCallsCount<MethodInfo>> TotalActivity { get; set; }

        /// <summary>
        /// Список текущего количества вызовов метода.
        /// </summary>
        /// <remarks>
        /// <see cref="MethodCallsCount{MethodInfo}"/>.
        /// </remarks>
        List<MethodCallsCount<MethodInfo>> CurrentActivity { get; set; }

        /// <summary>
        /// Дата аптайма сервиса.
        /// </summary>
        DateTime UptimeSince { get; set; }

        /// <summary>
        /// Имя класса метода.
        /// </summary>
        string ClassName { get; set; }

        /// <summary>
        /// Имена методов класса.
        /// </summary>
        List<string> MethodNames { get; set; }
    }
}
