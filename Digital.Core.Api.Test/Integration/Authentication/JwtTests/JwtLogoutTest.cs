using System.Net;
using Digital.Core.Api.Test.Api;
using Digital.Lib.Net.Authentication.Events;
using Digital.Lib.Net.Entities.Models.Events;
using Digital.Lib.Net.Entities.Models.Users;
using Digital.Lib.Net.TestTools.Integration;

namespace Digital.Core.Api.Test.Integration.Authentication.JwtTests;

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
