using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Swashbuckle.AspNetCore.Filters;
using Unchase.FluentPerformanceMeter.Models;

namespace Unchase.FluentPerformanceMeter.TestWebAPI31.OpenApiExamples
{
    internal class ResponseExamples
    {
        private class PerformanceInfoExample : IPerformanceInfo
        {
            public List<MethodCallInfo<MethodInfo>> MethodCalls { get => new List<MethodCallInfo<MethodInfo>>(); set { } }

            public List<MethodCallsCount<MethodInfo>> TotalActivity { get => new List<MethodCallsCount<MethodInfo>>(); set { } }

            public List<MethodCallsCount<MethodInfo>> CurrentActivity { get => new List<MethodCallsCount<MethodInfo>>(); set { } }

            public DateTime UptimeSince { get => DateTime.Now.AddHours(-1); set { } }

            public string ClassName { get => "SomeClassName"; set { } }

            public List<string> MethodNames { get => new List<string>(); set { } }

            public long TimerFrequency => Stopwatch.Frequency;

            IDictionary<string, object> IPerformanceInfo.CustomData { get => new Dictionary<string, object>(); }
        }

        internal class GetPerformanceInfoResponse200Example : IExamplesProvider<object>
        {
            /// <summary>
            /// Get examples.
            /// </summary>
            public object GetExamples()
            {
                return new PerformanceInfoExample();
            }
        }
    }
}
