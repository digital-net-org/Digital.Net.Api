using Digital.Net.Core.Entities.Context;
using Digital.Net.Tests.Core;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;

namespace Digital.Net.Core.Entities.Test.Seeds;

public class SeederTest : UnitTest, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SeederTestSeed _userSeeder;
    private readonly DigitalContext _context;

    public SeederTest()
    {
        _connection = SqliteInMemoryHelper.GetConnection();
        _context = _connection.CreateContext<DigitalContext>();
        _userSeeder = new SeederTestSeed(
            new Mock<ILogger<SeederTestSeed>>().Object,
            _context
        );
    }

    [Test]
    public async Task SeedAsync_AddEntitiesToDatabase()
    {
        var result = await _userSeeder.SeedAsync(SeederTestSeed.Users);
        var users = _context.Users.ToList();
        await Assert.That(result.HasError).IsFalse();
        await Assert.That(users.Count).IsEqualTo(2);
    }

    [Test]
    public async Task SeedAsync_SkipExistingEntities()
    {
        await _context.Users.AddAsync(SeederTestSeed.Users[0]);
        await _context.SaveChangesAsync();

        var result = await _userSeeder.SeedAsync([SeederTestSeed.Users[0]]);
        await Assert.That(result.HasError).IsFalse();
        await Assert.That(_context.Users.Where(u => u.Username == SeederTestSeed.Users[0].Username)).HasSingleItem();
    }

    [Test]
    public async Task SeedAsync_ReturnsSeededEntities()
    {
        await _context.Users.AddAsync(SeederTestSeed.Users[0]);
        await _context.SaveChangesAsync();

        var result = await _userSeeder.SeedAsync(SeederTestSeed.Users);
        var user = result.Value!.Find(u => u.Username == SeederTestSeed.Users[1].Username);
        await Assert.That(result.HasError).IsFalse();
        await Assert.That(result.Value!).HasSingleItem();
        await Assert.That(user is not null && user.Id != Guid.Empty).IsTrue();
    }

    public void Dispose() => _connection.Dispose();
}