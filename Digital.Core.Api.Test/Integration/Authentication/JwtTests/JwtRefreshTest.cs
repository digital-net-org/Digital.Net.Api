using System.Net;
using Digital.Core.Api.Test.Collections;
using Digital.Lib.Net.Core.Extensions.StringUtilities;
using Digital.Lib.Net.Http.HttpClient.Extensions;
using Digital.Lib.Net.TestTools.Integration;

namespace Digital.Core.Api.Test.Integration.Authentication.JwtTests;

public class JwtRefreshTest(AppFactory<Program> fixture) : AuthenticationTest(fixture)
{
     [Fact]
     public async Task RefreshTokens_WithValidRefreshToken_ShouldReturnToken()
     {
         var user = GetUser();
         await BaseClient.Login(user);
         var response = await BaseClient.RefreshTokens();
         var userTokens = GetUserTokens(user).ToList();
         var cookieToken = response.Headers.TryGetCookie(CookieName);

         Assert.Equal(HttpStatusCode.OK, response.StatusCode);
         Assert.Equal(cookieToken, userTokens.First().Key);
         Assert.Single(userTokens);

         foreach (var token in new List<string?> { cookieToken, await response.Content.ReadAsStringAsync() })
             Assert.True(token!.IsJsonWebToken());
     }

     [Fact]
     public async Task RefreshTokens_WithInvalidToken_ShouldReturnUnauthorized()
     {
         var user = GetUser();
         await BaseClient.Login(user);
         await BaseClient.Logout();
         var response = await BaseClient.RefreshTokens();
         Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
     }
}
