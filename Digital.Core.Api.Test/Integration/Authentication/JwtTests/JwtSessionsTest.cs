using System.Net;
using Digital.Core.Api.Test.Api;
using Digital.Lib.Net.Authentication.Events;
using Digital.Lib.Net.Authentication.Options;
using Digital.Lib.Net.Entities.Models.Events;
using Digital.Lib.Net.TestTools.Integration;

namespace Digital.Core.Api.Test.Integration.Authentication.JwtTests;

public class JwtSessionsTest(AppFactory<Program> fixture) : AuthenticationTest(fixture)
{
    [Fact]
     public async Task Login_OnMaxCurrentSessions_ShouldInvalidateOldestSession()
     {
         const int maxSessions = DefaultAuthenticationOptions.MaxConcurrentSessions;
         var user = GetUser();
         
         CreateClient(maxSessions + 1);
         foreach (var client in ClientPool)
             await client.Login(user);

         var successCount = await EventRepository.CountAsync(
             e => e.UserId == user.Id
             && e.Name == AuthenticationEvents.Login
             && e.State == EventState.Success
         );
         
         Assert.Equal(maxSessions + 1, successCount);
         Assert.Equal(HttpStatusCode.Unauthorized, (await ClientPool.First().RefreshTokens()).StatusCode);
     }
}
