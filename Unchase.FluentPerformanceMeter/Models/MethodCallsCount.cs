﻿using System.Reflection;
using System.Runtime.Serialization;

namespace Unchase.FluentPerformanceMeter.Models
{
    /// <summary>
    /// Method calls count information.
    /// </summary>
    /// <typeparam name="T">Method with type <see cref="MethodInfo"/>.</typeparam>
    [DataContract]
    public class MethodCallsCount<T> where T : MethodInfo
    {
        #region Properties

        /// <summary>
        /// Method information.
        /// </summary>
        /// <remarks>
        /// <see cref="MethodInfo"/>.
        /// </remarks>
        public T Method { get; }

        /// <summary>
        /// Method name.
        /// </summary>
        [DataMember]
        public string MethodName => Method?.Name;

        internal long _callsCount;

        /// <summary>
        /// Method calls count.
        /// </summary>
        [DataMember]
        public long CallsCount
        {
            get => _callsCount;
            private set => _callsCount = value;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for <see cref="MethodCallsCount{T}"/>.
        /// </summary>
        internal MethodCallsCount() { }

        /// <summary>
        /// Constructor for <see cref="MethodCallsCount{T}"/>.
        /// </summary>
        /// <param name="m"><see cref="Method"/>.</param>
        internal MethodCallsCount(T m)
        {
            Method = m;
        }

        #endregion
    }
}
