using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Safari.Net.Core.Messages;
using Safari.Net.Core.Random;
using Safari.Net.TestTools;
using SafariDigital.Core.Application;
using SafariDigital.Services.JwtService;
using Tests.Utils.Factories;

namespace Tests.SafariDigital.Services.JwtService;

public class JwtServiceTest : UnitTest
{
    private readonly global::SafariDigital.Services.JwtService.JwtService _jwtService;

    public JwtServiceTest()
    {
        var securityKey = JwtUtils.GetSecurityKey(Randomizer.GenerateRandomString());
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
        var content = UserFactoryUtils.CreateUser();
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
        var content = UserFactoryUtils.CreateUser();
        var token = _jwtService.GenerateBearerToken(content);

        // Act
        var result = _jwtService.ValidateToken($"{token}:invalid");

        // Assert
        Assert.Collection(result.Errors, e => Assert.IsType<ResultMessage>(e));
        Assert.True(result.HasError);
    }
}