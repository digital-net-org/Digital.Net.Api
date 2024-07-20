using Microsoft.Extensions.Configuration;
using SafariDigital.Core.Application;
using SafariDigital.Core.Random;
using SafariDigital.Core.Validation;
using SafariDigital.Services.Jwt;
using Tests.Core.Base;

namespace Tests.Unit.SafariDigital.Services.Jwt;

public class JwtServiceTest : UnitTest
{
    private readonly JwtService _jwtService;
    private string _secret;

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
        _jwtService = new JwtService(configuration);
    }

    [Fact]
    public void ValidateToken_ReturnsExpectedResult()
    {
        // Arrange
        var content = new TokenContent { Id = 1, Name = "test" };
        var token = _jwtService.GenerateBearerToken(content);

        // Act
        var result = _jwtService.ValidateToken<TokenContent>(token);

        // Assert
        Assert.Equal(token, result.Token);
        Assert.Equal(content.Id, result.Content!.Id);
        Assert.Equal(content.Name, result.Content!.Name);
        Assert.Empty(result.Errors);
        Assert.False(result.HasError);
    }

    [Fact]
    public void ValidateToken_ReturnsError()
    {
        // Arrange
        var content = new TokenContent { Id = 1, Name = "test" };
        var token = _jwtService.GenerateBearerToken(content);

        // Act
        var result = _jwtService.ValidateToken<TokenContent>($"{token}:invalid");

        // Assert
        Assert.Collection(result.Errors, e => Assert.IsType<ResultMessage>(e));
        Assert.True(result.HasError);
    }

    [Fact]
    public void GenerateBearerToken_ReturnsExpectedResult()
    {
        // Arrange
        var content = new { test = "test" };

        // Act
        var token = _jwtService.GenerateBearerToken(content);

        // Assert
        Assert.NotNull(token);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsExpectedResult()
    {
        // Arrange
        var content = new { test = "test" };

        // Act
        var token = _jwtService.GenerateRefreshToken(content);

        // Assert
        Assert.NotNull(token);
    }

    private class TokenContent
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}