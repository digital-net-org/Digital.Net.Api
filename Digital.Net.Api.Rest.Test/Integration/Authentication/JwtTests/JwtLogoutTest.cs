using System.Net;
using Digital.Net.Api.Entities.Models.Events;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Rest.Test.Api;
using Digital.Net.Api.Services.Authentication.Events;
using Digital.Net.Api.TestUtilities.Integration;

namespace Digital.Net.Api.Rest.Test.Integration.Authentication.JwtTests;

public class JwtLogoutTest(AppFactory<Program> fixture) : AuthenticationTest(fixture)
{
    [Fact]
     public async Task Logout_ShouldLogoutClient()
     {
         var user = GetUser();
         await BaseClient.Login(user);

         ExecuteTest(
             await BaseClient.Logout(),
             user,
             AuthenticationEvents.Logout
         );
     }

     [Fact]
     public async Task LogoutAll_ShouldLogoutAllClients()
     {
         var user = GetUser();
         await BaseClient.Login(user);
         await CreateClient().Login(user);

         ExecuteTest(
             await BaseClient.LogoutAll(),
             user,
             AuthenticationEvents.LogoutAll
         );
     }

     private void ExecuteTest(
         HttpResponseMessage result,
         User user,
         string eventType
     )
     {
         var logoutEvent = GetUserEvents(user).First();
         Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
         Assert.Equal(eventType, logoutEvent.Name);
         Assert.Equal(EventState.Success, logoutEvent.State);
         Assert.Empty(GetUserTokens(user));
     }

}
