using System.Net;
using Microsoft.AspNetCore.Http;
using Moq;
using SafariDigital.Services.HttpContextService;
using Tests.Core.Base;

namespace Tests.Unit.SafariDigital.Services.HttpContextService;

public class HttpContextUtilsTest : UnitTest
{
    [Fact]
    public void GetRemoteIpAddressFromRequest_ReturnsExpectedIpAddress()
    {
        // Arrange
        var headers = new HeaderDictionary
        {
            { "X-Forwarded-For", "192.168.1.1" }
        };

        var connectionMock = new Mock<ConnectionInfo>();
        connectionMock.Setup(m => m.RemoteIpAddress).Returns(IPAddress.Parse("192.168.1.2"));

        var httpContextMock = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
        httpContextMock.Setup(m => m.Connection).Returns(connectionMock.Object);

        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(m => m.Headers).Returns(headers);
        requestMock.Setup(m => m.HttpContext).Returns(httpContextMock.Object);

        // Act
        var ipAddress = HttpContextUtils.GetRemoteIpAddressFromRequest(requestMock.Object);

        // Assert
        Assert.Equal("192.168.1.1", ipAddress);
    }

    [Fact]
    public void GetUserAgentFromRequest_ReturnsExpectedUserAgent()
    {
        // Arrange
        var headers = new HeaderDictionary
        {
            { "User-Agent", "Safari/2.0" }
        };

        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(m => m.Headers).Returns(headers);

        // Act
        var userAgent = HttpContextUtils.GetUserAgentFromRequest(requestMock.Object);

        // Assert
        Assert.Equal("Safari/2.0", userAgent);
    }
}