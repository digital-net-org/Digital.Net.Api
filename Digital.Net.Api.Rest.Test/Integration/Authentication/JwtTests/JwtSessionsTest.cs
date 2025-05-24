using System.Net;
using Digital.Net.Api.Entities.Models.Events;
using Digital.Net.Api.Rest.Test.Api;
using Digital.Net.Api.Services.Authentication.Events;
using Digital.Net.Api.Services.Authentication.Options;
using Digital.Net.Api.TestUtilities.Integration;

namespace Digital.Net.Api.Rest.Test.Integration.Authentication.JwtTests;

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
