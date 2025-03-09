using Digital.Core.Api.Test.Utils;
using Digital.Lib.Net.Entities.Context;
using Digital.Lib.Net.Entities.Models.ApiKeys;
using Digital.Lib.Net.Entities.Models.ApiTokens;
using Digital.Lib.Net.Entities.Models.Events;
using Digital.Lib.Net.Entities.Models.Users;
using Digital.Lib.Net.Entities.Repositories;
using Digital.Lib.Net.TestTools.Integration;

namespace Digital.Core.Api.Test.Integration.Authentication;

public class AuthenticationTest : IntegrationTest<Program>
{
    protected readonly string CookieName = $"{AppFactorySettings.TestSettings["Domain"] ?? throw new Exception()}_refresh";
    protected readonly string HeaderKey = $"{AppFactorySettings.TestSettings["Domain"] ?? throw new Exception()}_auth";

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
    
    protected User GetUser() => UserRepository.BuildTestUser();
    
    protected User GetInactiveUser() => UserRepository.BuildTestUser(new UserDto { IsActive = false });
}