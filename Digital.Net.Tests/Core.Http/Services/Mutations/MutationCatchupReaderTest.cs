using Digital.Net.Cms.Context;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Mutations;
using Digital.Net.Core.Entities.Mutations;
using Digital.Net.Core.Http.Services.Mutations;
using Digital.Net.Tests.Core.Factories;
using TUnit.Core.Interfaces;

namespace Digital.Net.Tests.Core.Http.Services.Mutations;

public class MutationCatchupReaderTest : UnitTest, IAsyncInitializer
{
    [ClassDataSource<DatabaseFixture>]
    public required DatabaseFixture DbFixture { get; init; }

    private DigitalContext _digital = null!;
    private string _digitalType = null!;
    private string _cmsType = null!;
    private readonly Guid _digitalId = Guid.NewGuid();
    private readonly Guid _cmsId = Guid.NewGuid();

    public async Task InitializeAsync()
    {
        await DbFixture.EnsureCreatedAsync<CmsContext>();
        _digital = DbFixture.CreateContext<DigitalContext>();
        _digitalType = $"CatchupD-{TestId}";
        _cmsType = $"CatchupC-{TestId}";

        _digital.Set<EntityMutation>().Add(new EntityMutation
            { ChangeType = ChangeType.Created, EntityType = _digitalType, EntityId = _digitalId });
        await _digital.SaveChangesAsync();

        await using var cms = DbFixture.CreateContext<CmsContext>();
        cms.Set<EntityMutation>().Add(new EntityMutation
            { ChangeType = ChangeType.Updated, EntityType = _cmsType, EntityId = _cmsId });
        await cms.SaveChangesAsync();
    }

    private MutationCatchupReader BuildReader() =>
        new(_digital, [new MutationSchema(DigitalContext.Schema), new MutationSchema(CmsContext.Schema)]);

    [Test]
    public async Task ReadSince_AggregatesBothSchemas()
    {
        var reader = BuildReader();
        var result = await reader.ReadSinceAsync(null, new HashSet<string> { _digitalType, _cmsType },
            CancellationToken.None);

        await Assert.That(result.Any(s => s.EntityId == _digitalId)).IsTrue();
        await Assert.That(result.Any(s => s.EntityId == _cmsId)).IsTrue();
    }

    [Test]
    public async Task ReadSince_FiltersByEntityType()
    {
        var reader = BuildReader();
        var result = await reader.ReadSinceAsync(null, new HashSet<string> { _digitalType }, CancellationToken.None);

        await Assert.That(result.All(s => s.EntityType == _digitalType)).IsTrue();
        await Assert.That(result.Any(s => s.EntityId == _cmsId)).IsFalse();
    }

    [Test]
    public async Task ReadSince_CursorAfterAll_ReturnsNone()
    {
        var reader = BuildReader();
        var future = new MutationCursor(DateTime.UtcNow.AddMinutes(5), Guid.Empty);
        var result = await reader.ReadSinceAsync(future, new HashSet<string> { _digitalType, _cmsType },
            CancellationToken.None);

        await Assert.That(result).IsEmpty();
    }
}