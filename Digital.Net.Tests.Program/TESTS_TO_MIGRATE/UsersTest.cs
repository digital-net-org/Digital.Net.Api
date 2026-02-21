using System.Collections.Generic;
using System.Linq;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Entities.Repositories;

namespace Digital.Net.Tests.Program.TESTS_TO_MIGRATE;

public abstract class UsersTest
{
    protected readonly IRepository<User, DigitalContext> UserRepository;

    protected List<User> UserPool => UserRepository.Get().ToList();

    // protected User GetUser() => UserRepository.BuildTestUser();

    private void ResetUserPool()
    {
        foreach (var user in UserPool)
            UserRepository.Delete(u => u.Id == user.Id);

        UserRepository.Save();
        UserPool.Clear();
    }
}