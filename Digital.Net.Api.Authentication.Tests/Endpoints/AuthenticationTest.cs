using Digital.Net.Api.Core.Settings;
using Digital.Net.Api.Entities.Models.ApiTokens;
using Digital.Net.Api.Entities.Models.Events;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Tests.Core;
using Digital.Net.Tests.Core.Factories.Data.Records;

namespace Digital.Net.Api.Authentication.Tests.Endpoints;

public abstract class AuthenticationTest : IntegrationTest
{
    protected string CookieName =>
        $"{Application.GetConfiguration<string>(AppSettings.DomainKey) ?? throw new Exception()}_refresh";

    protected string HeaderKey =>
        $"{Application.GetConfiguration<string>(AppSettings.DomainKey) ?? throw new Exception()}_auth";

    protected IEnumerable<ApiToken> GetUserTokens(User user) =>
        Application
            .GetRepository<ApiToken>()
            .Get(x => x.UserId == user.Id);

    protected IEnumerable<Event> GetUserEvents(User user) =>
        Application
            .GetRepository<Event>()
            .Get(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAt);

    protected User GetInactiveUser() => Application.CreateUser(new TestUserPayload { IsActive = false });
}