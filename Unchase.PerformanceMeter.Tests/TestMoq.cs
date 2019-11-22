using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;

namespace Unchase.PerformanceMeter.Tests
{
    public class TestMoq
    {
        private static Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        public static Mock<IHttpContextAccessor> MockHttpContextAccessor
        {
            get
            {
                if (_mockHttpContextAccessor == null)
                {
                    var mockConnectionInfo = new Mock<ConnectionInfo>();
                    mockConnectionInfo.Setup(_ => _.RemoteIpAddress).Returns(IPAddress.Parse("127.0.0.1"));
                    _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
                    _mockHttpContextAccessor.Setup(_ => _.HttpContext.Connection).Returns(mockConnectionInfo.Object);
                }
                return _mockHttpContextAccessor;
            }
        }
    }
}
