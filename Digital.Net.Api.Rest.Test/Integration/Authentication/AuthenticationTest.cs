using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.ApiKeys;
using Digital.Net.Api.Entities.Models.ApiTokens;
using Digital.Net.Api.Entities.Models.Events;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.TestUtilities.Integration;

namespace Digital.Net.Api.Rest.Test.Integration.Authentication;

public abstract class AuthenticationTest : IntegrationTest<Program>
{
    protected readonly string CookieName =
        $"{AppFactorySettings.TestSettings["Domain"] ?? throw new Exception()}_refresh";
    protected readonly string HeaderKey =
        $"{AppFactorySettings.TestSettings["Domain"] ?? throw new Exception()}_auth";

    protected readonly IRepository<User, DigitalContext> UserRepository;
    protected readonly IRepository<Event, DigitalContext> EventRepository;
    protected readonly IRepository<ApiToken, DigitalContext> ApiTokenRepository;
    protected readonly IRepository<ApiKey, DigitalContext> ApiKeyRepository;

    protected AuthenticationTest(AppFactory<Program> fixture) : base(fixture)
    {
        UserRepository = GetRepository<User, DigitalContext>();
        EventRepository = GetRepository<Event, DigitalContext>();
        ApiKeyRepository = GetRepository<ApiKey, DigitalContext>();
        ApiTokenRepository = GetRepository<ApiToken, DigitalContext>();
    }
    
    protected IEnumerable<ApiToken> GetUserTokens(User user) =>
        ApiTokenRepository.Get(x => x.UserId == user.Id);

    protected IEnumerable<Event> GetUserEvents(User user) => 
        EventRepository
            .Get(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAt);
    
    protected User GetUser() => CreateUser();
    
    protected User GetInactiveUser() => CreateUser(new UserDto { IsActive = false });
}