using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Tests.Core;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;

namespace Digital.Net.Api.Entities.Test.Seeds;

public class SeederTest : UnitTest, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SeederTestSeed _userSeeder;
    private readonly Repository<User> _userRepository;

    public SeederTest()
    {
        _connection = SqliteInMemoryHelper.GetConnection();
        var context = _connection.CreateContext<DigitalContext>();
        _userRepository = new Repository<User>(context);
        _userSeeder = new SeederTestSeed(
            new Mock<ILogger<SeederTestSeed>>().Object,
            _userRepository
        );
    }

    [Test]
    public async Task SeedAsync_AddEntitiesToDatabase()
    {
        var result = await _userSeeder.SeedAsync(SeederTestSeed.Users);
        var users = _userRepository.Get(x => true);
        await Assert.That(result.HasError).IsFalse();
        await Assert.That(users.Count()).IsEqualTo(2);
    }

    [Test]
    public async Task SeedAsync_SkipExistingEntities()
    {
        await _userRepository.CreateAsync(SeederTestSeed.Users[0]);
        await _userRepository.SaveAsync();

        var result = await _userSeeder.SeedAsync([SeederTestSeed.Users[0]]);
        await Assert.That(result.HasError).IsFalse();
        await Assert.That(_userRepository.Get(u => u.Username == SeederTestSeed.Users[0].Username)).HasSingleItem();
    }

    [Test]
    public async Task SeedAsync_ReturnsSeededEntities()
    {
        await _userRepository.CreateAsync(SeederTestSeed.Users[0]);
        await _userRepository.SaveAsync();

        var result = await _userSeeder.SeedAsync(SeederTestSeed.Users);
        var user = result.Value!.Find(u => u.Username == SeederTestSeed.Users[1].Username);
        await Assert.That(result.HasError).IsFalse();
        await Assert.That(result.Value!).HasSingleItem();
        await Assert.That(user is not null && user.Id != Guid.Empty).IsTrue();
    }

    public void Dispose() => _connection.Dispose();
}