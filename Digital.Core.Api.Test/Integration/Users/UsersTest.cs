using Digital.Core.Api.Test.Utils;
using Digital.Lib.Net.Entities.Context;
using Digital.Lib.Net.Entities.Models.Users;
using Digital.Lib.Net.Entities.Repositories;
using Digital.Lib.Net.TestTools.Integration;

namespace Digital.Core.Api.Test.Integration.Users;

public class UsersTest : IntegrationTest<Program>
{
    protected readonly IRepository<User, DigitalContext> UserRepository;

    protected UsersTest(AppFactory<Program> fixture) : base(fixture)
    {
        UserRepository = GetRepository<User, DigitalContext>();
        ResetUserPool();
        for (var i = 1; i < 16; i++)
            UserPool.Add(UserRepository.BuildTestUser(new UserDto
            {
                Username = $"User{i}",
                Email = $"User{i}@email.com",
                Login = $"u{i}",
                IsActive = i != 15
            }));
    }

    protected List<User> UserPool => UserRepository.Get().ToList();

    protected User GetUser() => UserRepository.BuildTestUser();

    private void ResetUserPool()
    {
        foreach (var user in UserPool)
            UserRepository.Delete(u => u.Id == user.Id);

        UserRepository.Save();
        UserPool.Clear();
    }
}