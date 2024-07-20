using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using SafariDigital.Core.Validation;
using SafariDigital.Services.Jwt;
using SafariDigital.Services.Jwt.Http;
using SafariDigital.Services.Jwt.Models;

namespace Tests.Unit.SafariDigital.Services.Jwt.Http;

public class HttpContextExtensionTest
{
    private const string DefaultBearerToken = "test_token";
    private const string DefaultCookieName = "test_cookie";
    private const string DefaultBearerName = "Token";
    private const string RefreshTokenCookieName = "RefreshToken";

    private readonly Mock<HttpContext> _mockContext = new();
    private readonly Mock<IResponseCookies> _mockCookies = new();
    private readonly Mock<IJwtService> _mockJwtService = new();
    private readonly Mock<HttpRequest> _mockRequest = new();
    private readonly Mock<HttpResponse> _mockResponse = new();
    private readonly Mock<IServiceProvider> _mockServices = new();

    public HttpContextExtensionTest()
    {
        SetupMocks();
    }

    private void SetupMocks(HeaderDictionary? headers = null, string? cookieName = null, string? bearerName = null)
    {
        _mockServices.Setup(provider => provider.GetService(typeof(IJwtService)))
            .Returns(_mockJwtService.Object);
        _mockResponse.Setup(r => r.HttpContext.RequestServices).Returns(_mockServices.Object);
        _mockResponse.Setup(r => r.Cookies).Returns(_mockCookies.Object);
        _mockRequest.Setup(r => r.HttpContext.RequestServices).Returns(_mockServices.Object);
        _mockRequest.Setup(r => r.Headers).Returns(headers ?? new HeaderDictionary
            { { "Authorization", new StringValues($"Bearer {DefaultBearerToken}") } });
        _mockRequest.Setup(r => r.HttpContext.Items).Returns(new Dictionary<object, object>
            { { bearerName ?? DefaultBearerName, new JwtToken<object>() } }!);
        _mockRequest.Setup(r => r.Cookies)
            .Returns(Mock.Of<IRequestCookieCollection>(c =>
                c[cookieName ?? RefreshTokenCookieName] == DefaultBearerToken));
        _mockContext.Setup(c => c.Items).Returns(new Dictionary<object, object>
            { { bearerName ?? DefaultBearerName, new JwtToken<object>() } }!);
        _mockContext.Setup(c => c.Request).Returns(_mockRequest.Object);
        _mockJwtService.Setup(c => c.GetBearerTokenExpiration()).Returns(1000);
        _mockJwtService.Setup(c => c.GetCookieName()).Returns(cookieName ?? DefaultCookieName);
    }

    [Fact]
    public void GetBearerToken_Request_ReturnsToken()
    {
        var result = _mockRequest.Object.GetBearerToken();
        Assert.Equal(DefaultBearerToken, result);
    }

    [Fact]
    public void GetBearerToken_Context_ReturnsToken()
    {
        var result = _mockContext.Object.GetBearerToken();
        Assert.Equal(DefaultBearerToken, result);
    }

    [Fact]
    public void GetJwtToken_Context_ReturnsToken()
    {
        var result = _mockContext.Object.GetJwtToken<object>();
        Assert.NotNull(result);
    }

    [Fact]
    public void GetJwtToken_Context_ReturnsTokenWithError()
    {
        SetupMocks(bearerName: "WrongSpelledToken");
        var result = _mockContext.Object.GetJwtToken<object>();
        Assert.NotNull(result);
        Assert.True(result.HasError);
        Assert.Collection(result.Errors, e => Assert.IsType<ResultMessage>(e));
    }

    [Fact]
    public void GetJwtToken_Request_ReturnsToken()
    {
        var result = _mockRequest.Object.GetJwtToken<object>();
        Assert.NotNull(result);
    }

    [Fact]
    public void GetCookieToken_ReturnsToken()
    {
        SetupMocks(cookieName: RefreshTokenCookieName);
        var result = _mockRequest.Object.GetCookieToken();
        Assert.Equal(DefaultBearerToken, result);
    }

    [Fact]
    public void SetCookieToken_SetsCookie()
    {
        _mockResponse.Object.SetCookieToken(DefaultBearerToken);
        _mockCookies.Verify(c => c.Append(DefaultCookieName, DefaultBearerToken, It.IsAny<CookieOptions>()),
            Times.Once);
    }

    [Fact]
    public void RemoveCookieToken_RemovesCookie()
    {
        _mockResponse.Object.RemoveCookieToken();
        _mockCookies.Verify(c => c.Delete(DefaultCookieName), Times.Once);
    }
}