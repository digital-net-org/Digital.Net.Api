using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Tests.Core;
using Digital.Net.Tests.Core.Factories;
using Microsoft.Extensions.Logging;
using Moq;
using TUnit.Core.Interfaces;

namespace Digital.Net.Tests.Entities.Seeds;

public class SeederTest : UnitTest, IAsyncInitializer
{
    [ClassDataSource<DatabaseFixture>]
    public required DatabaseFixture DbFixture { get; init; }

    private DigitalContext _context = null!;
    private SeederTestSeed _userSeeder = null!;
    private List<User> _users = null!;

    public async Task InitializeAsync()
    {
        _context = DbFixture.CreateContext<DigitalContext>();
        _userSeeder = new SeederTestSeed(
            new Mock<ILogger<SeederTestSeed>>().Object,
            _context
        );
        _users = SeederTestSeed.CreateUsers(TestId);
    }

    [Test]
    public async Task SeedAsync_AddEntitiesToDatabase()
    {
        var result = await _userSeeder.SeedAsync(_users);
        var seeded = _context.Users.Where(u => u.Login.EndsWith(TestId)).ToList();
        await Assert.That(result.HasError).IsFalse();
        await Assert.That(seeded.Count).IsEqualTo(2);
    }

    [Test]
    public async Task SeedAsync_SkipExistingEntities()
    {
        await _context.Users.AddAsync(_users[0]);
        await _context.SaveChangesAsync();

        var result = await _userSeeder.SeedAsync([_users[0]]);
        await Assert.That(result.HasError).IsFalse();
        await Assert.That(_context.Users.Where(u => u.Username == _users[0].Username)).HasSingleItem();
    }

    [Test]
    public async Task SeedAsync_ReturnsSeededEntities()
    {
        await _context.Users.AddAsync(_users[0]);
        await _context.SaveChangesAsync();

        var result = await _userSeeder.SeedAsync(_users);
        var user = result.Value!.Find(u => u.Username == _users[1].Username);
        await Assert.That(result.HasError).IsFalse();
        await Assert.That(result.Value!).HasSingleItem();
        await Assert.That(user is not null && user.Id != Guid.Empty).IsTrue();
    }
}
