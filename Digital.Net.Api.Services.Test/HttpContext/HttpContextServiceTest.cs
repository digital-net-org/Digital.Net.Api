using System.Text.Json;
using Digital.Net.Api.Services.HttpContext;
using Digital.Net.Tests.Core;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Digital.Net.Api.Services.Test.HttpContext;

public class HttpContextServiceTest : UnitTest
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly DefaultHttpContext _httpContext = new();

    public HttpContextServiceTest()
    {
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(_httpContext);
    }

    [Test]
    public async Task AddItem_And_GetItem_Should_Work_Correctly()
    {
        var key = "my-key";
        var content = new { Name = "Test", Value = 123 };
        var service = new HttpContextService(_httpContextAccessorMock.Object);

        service.AddItem(key, content);
        var result = service.GetItem<dynamic>(key);

        await Assert
            .That((object)JsonSerializer.Serialize(result))
            .IsEqualTo(JsonSerializer.Serialize(content));
    }

    [Test]
    public async Task GetHeaderValue_Should_Return_HeaderValue()
    {
        var header = "X-My-Header";
        var value = "my-value";
        _httpContext.Request.Headers[header] = value;

        var service = new HttpContextService(_httpContextAccessorMock.Object);
        var result = service.GetHeaderValue(header);
        await Assert.That(result).IsEqualTo(value);
    }

    [Test]
    public async Task SetResponseCookie_Should_Set_Cookie()
    {
        var name = "my-cookie";
        var content = "my-cookie-value";
        var expiration = DateTime.Now.AddDays(1);
        var service = new HttpContextService(_httpContextAccessorMock.Object);
        service.SetResponseCookie(content, name, expiration);
        var cookie = _httpContext.Response.Headers["Set-Cookie"].ToString();

        await Assert.That(cookie).IsNotNull();
        await Assert.That(cookie).Contains(name);
        await Assert.That(cookie).Contains(content);
    }
}