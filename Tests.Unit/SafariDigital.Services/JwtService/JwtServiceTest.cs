using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SafariDigital.Core.Application;
using SafariDigital.Core.Random;
using SafariDigital.Core.Validation;
using SafariDigital.Services.JwtService;
using Tests.Core.Base;
using Tests.Core.Factories;

namespace Tests.Unit.SafariDigital.Services.JwtService;

public class JwtServiceTest : UnitTest
{
    private readonly global::SafariDigital.Services.JwtService.JwtService _jwtService;

    public JwtServiceTest()
    {
        var securityKey = JwtUtils.GetSecurityKey(RandomUtils.GenerateRandomSecret());
        var configuration = new ConfigurationManager().AddProjectSettings().Build();
        foreach (var (key, value) in ((string, string)[])
                 [
                     ("Jwt:Issuer", "test"),
                     ("Jwt:Audience", "test"),
                     ("Jwt:Secret", securityKey.ToString()),
                     ("Jwt:CookieName", "test"),
                     ("Jwt:BearerTokenExpiration", "2000"),
                     ("Jwt:RefreshTokenExpiration", "20000")
                 ]) Environment.SetEnvironmentVariable(key, value);
        _jwtService = new global::SafariDigital.Services.JwtService.JwtService(new global::SafariDigital.Services.HttpContextService.HttpContextService(new HttpContextAccessor(), null), configuration);
    }

    [Fact]
    public void ValidateToken_ReturnsExpectedResult()
    {
        // Arrange
        var content = UserFactory.CreateUser();
        var token = _jwtService.GenerateBearerToken(content);

        // Act
        var result = _jwtService.ValidateToken(token);

        // Assert
        Assert.Equal(token, result.Token);
        Assert.Equal(content.Id, result.Content!.Id);
        Assert.Equal(content.Role, result.Content!.Role);
        Assert.Empty(result.Errors);
        Assert.False(result.HasError);
    }

    [Fact]
    public void ValidateToken_ReturnsError()
    {
        // Arrange
        var content = UserFactory.CreateUser();
        var token = _jwtService.GenerateBearerToken(content);

        // Act
        var result = _jwtService.ValidateToken($"{token}:invalid");

        // Assert
        Assert.Collection(result.Errors, e => Assert.IsType<ResultMessage>(e));
        Assert.True(result.HasError);
    }
}