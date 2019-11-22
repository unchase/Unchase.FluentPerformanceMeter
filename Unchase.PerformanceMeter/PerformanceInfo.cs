using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Unchase.PerformanceMeter
{
    /// <summary>
    /// Сведения о производительности метода.
    /// </summary>
    /// <typeparam name="TClass">Класс с методами.</typeparam>
    [DataContract]
    internal class PerformanceInfo<TClass> : IPerformanceInfo where TClass : class
    {
        #region Properties

        /// <summary>
        /// Список вызовов метода.
        /// </summary>
        /// <remarks>
        /// <see cref="MethodCallInfo{MethodInfo}"/>.
        /// </remarks>
        [DataMember]
        public List<MethodCallInfo<MethodInfo>> MethodCalls { get; set; }

        /// <summary>
        /// Список общего количества вызовов метода.
        /// </summary>
        /// <remarks>
        /// <see cref="MethodCallsCount{MethodInfo}"/>.
        /// </remarks>
        [DataMember]
        public List<MethodCallsCount<MethodInfo>> TotalActivity { get; set; }

        /// <summary>
        /// Список текущего количества вызовов метода.
        /// </summary>
        /// <remarks>
        /// <see cref="MethodCallsCount{MethodInfo}"/>.
        /// </remarks>
        [DataMember]
        public List<MethodCallsCount<MethodInfo>> CurrentActivity { get; set; }

        /// <summary>
        /// Дата аптайма сервиса.
        /// </summary>
        [DataMember]
        public DateTime UptimeSince { get; set; }

        /// <summary>
        /// Имя класса метода.
        /// </summary>
        [DataMember]
        public string ClassName
        {
            get
            {
                return typeof(TClass).FullName;
            }
            set { }
        }

        /// <summary>
        /// Имена методов класса.
        /// </summary>
        [DataMember]
        public List<string> MethodNames { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Конструктор класса <see cref="PerformanceInfo{TClass}"/>.
        /// </summary>
        public PerformanceInfo()
        {
            MethodCalls = new List<MethodCallInfo<MethodInfo>>();
            TotalActivity = new List<MethodCallsCount<MethodInfo>>();
            CurrentActivity = new List<MethodCallsCount<MethodInfo>>();
            UptimeSince = DateTime.Now;
            var methodInfos = typeof(TClass).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(mi => !mi.IsSpecialName).ToArray();
            MethodNames = methodInfos.Select(mi => mi.Name).ToList();
            foreach (var method in methodInfos)
            {
                TotalActivity.Add(new MethodCallsCount<MethodInfo>(method));
                CurrentActivity.Add(new MethodCallsCount<MethodInfo>(method));
            }
        }

        #endregion
    }

    /// <summary>
    /// Сведения о вызовах метода.
    /// </summary>
    /// <typeparam name="T">Метод типа <see cref="MethodInfo"/>.</typeparam>
    [DataContract]
    public class MethodCallInfo<T> where T : MethodInfo
    {
        #region Properties

        /// <summary>
        /// Метод.
        /// </summary>
        /// <remarks>
        /// <see cref="MethodInfo"/>.
        /// </remarks>
        public T Method { get; set; }

        /// <summary>
        /// Имя метода.
        /// </summary>
        [DataMember]
        public string MethodName { get; set; }

        /// <summary>
        /// Длительность вызова метода в миллисекундах.
        /// </summary>
        [DataMember]
        public long DurationMiliseconds { get; set; }

        /// <summary>
        /// Вызывающмй клиент.
        /// </summary>
        [DataMember]
        public string Caller { get; set; }

        /// <summary>
        /// Дата начала вызова метода.
        /// </summary>
        [DataMember]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Дата окончания вызова метода.
        /// </summary>
        [DataMember]
        public DateTime EndTime { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Стандартный конструктор класса <see cref="DataContracts.MethodCallInfo{T}"/>.
        /// </summary>
        public MethodCallInfo() { }

        /// <summary>
        /// Конструктор класса <see cref="DataContracts.MethodCallInfo{T}"/>.
        /// </summary>
        /// <param name="m"><see cref="Method"/>.</param>
        /// <param name="duration"><see cref="DurationMiliseconds"/>.</param>
        /// <param name="caller"><see cref="Caller"/>.</param>
        /// <param name="ds"><see cref="StartTime"/>.</param>
        /// <param name="de"><see cref="EndTime"/>.</param>
        public MethodCallInfo(T m, long duration, string caller, DateTime ds, DateTime de)
        {
            Method = m;
            if (m != null)
                MethodName = m.Name;
            DurationMiliseconds = duration;
            Caller = caller;
            StartTime = ds;
            EndTime = de;
        }

        #endregion
    }

    /// <summary>
    /// Сведения о количестве вызовов метода.
    /// </summary>
    /// <typeparam name="T">Метод типа <see cref="MethodInfo"/>.</typeparam>
    [DataContract]
    public class MethodCallsCount<T> where T : MethodInfo
    {
        #region Properties

        /// <summary>
        /// Метод.
        /// </summary>
        /// <remarks>
        /// <see cref="Enum"/>.
        /// </remarks>
        public T Method { get; set; }

        /// <summary>
        /// Имя метода.
        /// </summary>
        [DataMember]
        public string MethodName { get; set; }

        /// <summary>
        /// Количество вызовов.
        /// </summary>
        [DataMember]
        public long CallsCount { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Стандартный конструктор класса <see cref="MethodCallsCount{T}"/>.
        /// </summary>
        public MethodCallsCount() { }

        /// <summary>
        /// Конструктор класса <see cref="MethodCallsCount{T}"/>.
        /// </summary>
        /// <param name="m"><see cref="Method"/>.</param>
        public MethodCallsCount(T m)
        {
            Method = m;
            if (m != null)
                MethodName = m.Name;
        }

        #endregion
    }
}
