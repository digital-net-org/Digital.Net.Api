using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.TestUtilities;
using Digital.Net.Api.TestUtilities.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;

namespace Digital.Net.Api.Entities.Test.Seeds;

public class SeederTest : UnitTest, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SeederTestSeed _userSeeder;
    private readonly Repository<User, DigitalContext> _userRepository;

    public SeederTest()
    {
        _connection = SqliteInMemoryHelper.GetConnection();
        var context = _connection.CreateContext<DigitalContext>();
        _userRepository = new Repository<User, DigitalContext>(context);
        _userSeeder = new SeederTestSeed(
            new Mock<ILogger<SeederTestSeed>>().Object,
            _userRepository
        );
    }

    [Fact]
    public async Task SeedAsync_AddEntitiesToDatabase()
    {
        var result = await _userSeeder.SeedAsync(SeederTestSeed.Users);
        var users = _userRepository.Get(x => true);
        Assert.False(result.HasError);
        Assert.Equal(2, users.Count());
    }

    [Fact]
    public async Task SeedAsync_SkipExistingEntities()
    {
        await _userRepository.CreateAsync(SeederTestSeed.Users[0]);
        await _userRepository.SaveAsync();

        var result = await _userSeeder.SeedAsync([SeederTestSeed.Users[0]]);
        Assert.False(result.HasError);
        Assert.Single(_userRepository.Get(u => u.Username == SeederTestSeed.Users[0].Username));
    }

    [Fact]
    public async Task SeedAsync_ReturnsSeededEntities()
    {
        await _userRepository.CreateAsync(SeederTestSeed.Users[0]);
        await _userRepository.SaveAsync();

        var result = await _userSeeder.SeedAsync(SeederTestSeed.Users);
        var user = result.Value!.Find(u => u.Username == SeederTestSeed.Users[1].Username);
        Assert.False(result.HasError);
        Assert.Single(result.Value!);
        Assert.True(user is not null && user.Id != Guid.Empty);
    }

    public void Dispose() => _connection.Dispose();
}