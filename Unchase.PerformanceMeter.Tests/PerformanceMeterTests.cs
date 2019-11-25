using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Unchase.PerformanceMeter.Tests
{
    public class PerformanceMeterTests
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PerformanceMeterTests()
        {
            _httpContextAccessor = TestMoq.MockHttpContextAccessor.Object;
        }

        public class PublicClass
        {
            public void PublicVoidMethod() { }

            public static void PublicStaticVoidMethod() { }

            private void PrivateVoidMethod() { }

            private static void PrivateStaticVoidMethod() { }

            internal void InternalVoidMethod() { }

            internal static void InternalStaticVoidMethod() { }
        }

        public static class PublicStaticClass
        {
            public static void PublicStaticVoidMethod() { }

            private static void PrivateStaticVoidMethod() { }

            internal static void InternalStaticVoidMethod() { }
        }

        [Fact]
        public void PerformanceMeterStartWaychingPublicVoidMethodWithHttpContectAccessorReturnsSuccess()
        {
            // Arrange
            PerformanceMeter<PublicClass> performanceMeter = null;
            
            // Act
            using (performanceMeter = PerformanceMeter<PublicClass>
                .Watching(nameof(PublicClass.PublicVoidMethod))
                .WithHttpContextAccessor(_httpContextAccessor)
                //.WithCaller("test")
                .Start())
            {
                // Arrange
                var performanceInfo = PerformanceMeter<PublicClass>.GetPerformanceInfo();
                var methodCurrentActivity = performanceInfo.CurrentActivity.Find(ca => ca.Method == performanceMeter.MethodInfo);
                var methodTotalActivity = performanceInfo.TotalActivity.Find(ca => ca.Method == performanceMeter.MethodInfo);
                var methodCalls = performanceInfo.MethodCalls.Find(ca => ca.Method == performanceMeter.MethodInfo);

                // Assert
                performanceInfo.ClassName.Should().EndWith(typeof(PublicClass).Name, "class name should be known");

                performanceInfo.MethodNames.Count.Should().Be(typeof(PublicClass)
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                    .Where(mi => !mi.IsSpecialName && mi.GetCustomAttribute<IgnoreMethodPerformanceAttribute>() == null)
                    .Count());

                methodCurrentActivity.Should().NotBeNull();
                methodCurrentActivity.CallsCount.Should().Be(1);

                methodTotalActivity.Should().NotBeNull();
                methodTotalActivity.CallsCount.Should().Be(0);

                methodCalls.Should().BeNull();
            }

            // Arrange
            var performanceInfoAfterDispose = PerformanceMeter<PublicClass>.GetPerformanceInfo();
            var methodCurrentActivityAfterDispose = performanceInfoAfterDispose.CurrentActivity.Find(ca => ca.Method == performanceMeter.MethodInfo);
            var methodTotalActivityAfterDispose = performanceInfoAfterDispose.TotalActivity.Find(ca => ca.Method == performanceMeter.MethodInfo);
            var methodCallsAfterDispose = performanceInfoAfterDispose.MethodCalls.Find(ca => ca.Method == performanceMeter.MethodInfo);

            // Assert
            methodCurrentActivityAfterDispose.Should().NotBeNull();
            methodCurrentActivityAfterDispose.CallsCount.Should().Be(0);

            methodTotalActivityAfterDispose.Should().NotBeNull();
            methodTotalActivityAfterDispose.CallsCount.Should().Be(1);

            methodCallsAfterDispose.Should().NotBeNull();
            methodCallsAfterDispose.Method.Should().BeSameAs(performanceMeter.MethodInfo);
            methodCallsAfterDispose.MethodName.Should().Be(nameof(PublicClass.PublicVoidMethod));
            methodCallsAfterDispose.DurationMiliseconds.Should().BeGreaterThan(0);
            methodCallsAfterDispose.Caller.Should().Be(_httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString());
        }
    }
}
