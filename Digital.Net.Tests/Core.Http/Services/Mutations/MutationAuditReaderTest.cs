using Digital.Net.Cms.Context;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Mutations;
using Digital.Net.Core.Entities.Mutations;
using Digital.Net.Core.Http.Services.Mutations;
using Digital.Net.Core.Http.Services.Mutations.Exceptions;
using Digital.Net.Tests.Core.Factories;
using TUnit.Core.Interfaces;

namespace Digital.Net.Tests.Core.Http.Services.Mutations;

public class MutationAuditReaderTest : UnitTest, IAsyncInitializer
{
    [ClassDataSource<DatabaseFixture>]
    public required DatabaseFixture DbFixture { get; init; }

    private DigitalContext _digital = null!;
    private string _entityType = null!;
    private string _restrictedType = null!;
    private string _unknownType = null!;
    private readonly Guid _digitalEntityId = Guid.NewGuid();
    private readonly Guid _cmsEntityId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private Guid _digitalRowId;
    private Guid _cmsRowId;
    private Guid _restrictedRowId;
    private DateTime _digitalCreatedAt;

    public async Task InitializeAsync()
    {
        await DbFixture.EnsureCreatedAsync<CmsContext>();
        _digital = DbFixture.CreateContext<DigitalContext>();
        _entityType = $"Audit-{TestId}";
        _restrictedType = $"Restricted-{TestId}";
        _unknownType = $"Unknown-{TestId}";

        var digitalRow = new EntityMutation
        {
            ChangeType = ChangeType.Created, EntityType = _entityType, EntityId = _digitalEntityId, UserId = _userId
        };
        _digital.Set<EntityMutation>().Add(digitalRow);
        await _digital.SaveChangesAsync();
        _digitalRowId = digitalRow.Id;
        _digitalCreatedAt = digitalRow.CreatedAt;

        await Task.Delay(50);
        await using var cms = DbFixture.CreateContext<CmsContext>();
        var cmsRow = new EntityMutation
        {
            ChangeType = ChangeType.Updated,
            EntityType = _entityType,
            EntityId = _cmsEntityId
        };
        cms.Set<EntityMutation>().Add(cmsRow);
        await cms.SaveChangesAsync();
        _cmsRowId = cmsRow.Id;

        // Admin-only type + a type absent from the whitelist (untracked / historical).
        var restrictedRow = new EntityMutation
            { ChangeType = ChangeType.Created, EntityType = _restrictedType, EntityId = Guid.NewGuid() };
        var unknownRow = new EntityMutation
            { ChangeType = ChangeType.Created, EntityType = _unknownType, EntityId = Guid.NewGuid() };
        _digital.Set<EntityMutation>().AddRange(restrictedRow, unknownRow);
        await _digital.SaveChangesAsync();
        _restrictedRowId = restrictedRow.Id;
    }

    // Synthetic whitelist: the reader only ever surfaces these two types (which also isolates the
    // shared EntityMutation table from other tests' rows).
    private MutationAuditReader BuildReader() =>
        new(
            _digital,
            [new MutationSchema(DigitalContext.Schema), new MutationSchema(CmsContext.Schema)],
            [new AuditedEntityType(_entityType, false), new AuditedEntityType(_restrictedType, true)]
        );

    [Test]
    public async Task Search_DefaultOrder_IsNewestFirst()
    {
        var page = await BuildReader().SearchAsync(
            new MutationAuditCriteria { EntityType = _entityType },
            CancellationToken.None
        );

        await Assert.That(page.Rows[0].Id).IsEqualTo(_cmsRowId);
        await Assert.That(page.Rows[1].Id).IsEqualTo(_digitalRowId);
    }

    [Test]
    public async Task Search_Paginates()
    {
        var page = await BuildReader().SearchAsync(
            new MutationAuditCriteria { EntityType = _entityType, Size = 1, Index = 2 },
            CancellationToken.None
        );

        await Assert.That(page.Total).IsEqualTo(2);
        await Assert.That(page.Rows.Count).IsEqualTo(1);
        await Assert.That(page.Rows[0].Id).IsEqualTo(_digitalRowId);
    }

    [Test]
    public async Task Search_FiltersByChangeType()
    {
        var page = await BuildReader().SearchAsync(
            new MutationAuditCriteria { EntityType = _entityType, ChangeType = (int)ChangeType.Created },
            CancellationToken.None
        );

        await Assert.That(page.Total).IsEqualTo(1);
        await Assert.That(page.Rows[0].Id).IsEqualTo(_digitalRowId);
    }

    [Test]
    public async Task Search_FiltersByEntityId()
    {
        var page = await BuildReader().SearchAsync(
            new MutationAuditCriteria { EntityId = _cmsEntityId },
            CancellationToken.None
        );

        await Assert.That(page.Total).IsEqualTo(1);
        await Assert.That(page.Rows[0].Id).IsEqualTo(_cmsRowId);
    }

    [Test]
    public async Task Search_FiltersByUserId()
    {
        var page = await BuildReader().SearchAsync(
            new MutationAuditCriteria { UserId = _userId },
            CancellationToken.None
        );

        await Assert.That(page.Total).IsEqualTo(1);
        await Assert.That(page.Rows[0].Id).IsEqualTo(_digitalRowId);
    }

    [Test]
    public async Task Search_ExcludesRestrictedTypes_ByDefault()
    {
        var page = await BuildReader().SearchAsync(new MutationAuditCriteria(), CancellationToken.None);

        await Assert.That(page.Total).IsEqualTo(2);
        await Assert.That(page.Rows.All(r => r.EntityType == _entityType)).IsTrue();
    }

    [Test]
    public async Task Search_IncludesRestrictedTypes_WhenPrivileged()
    {
        var page = await BuildReader().SearchAsync(
            new MutationAuditCriteria { IncludeRestricted = true },
            CancellationToken.None
        );

        await Assert.That(page.Total).IsEqualTo(3);
        await Assert.That(page.Rows.Any(r => r.Id == _restrictedRowId)).IsTrue();
    }

    [Test]
    public async Task Search_Throws_WhenRestrictedTypeRequestedWithoutPrivilege()
    {
        await Assert.ThrowsAsync<RestrictedAuditEntityException>(async () =>
            await BuildReader().SearchAsync(
                new MutationAuditCriteria { EntityType = _restrictedType },
                CancellationToken.None
            ));
    }

    [Test]
    public async Task Search_AllowsRestrictedTypeRequest_WhenPrivileged()
    {
        var page = await BuildReader().SearchAsync(
            new MutationAuditCriteria { EntityType = _restrictedType, IncludeRestricted = true },
            CancellationToken.None
        );

        await Assert.That(page.Total).IsEqualTo(1);
        await Assert.That(page.Rows[0].Id).IsEqualTo(_restrictedRowId);
    }

    [Test]
    public async Task Search_NeverReturnsTypesOutsideWhitelist()
    {
        var page = await BuildReader().SearchAsync(
            new MutationAuditCriteria { EntityType = _unknownType, IncludeRestricted = true },
            CancellationToken.None
        );

        await Assert.That(page.Total).IsEqualTo(0);
        await Assert.That(page.Rows).IsEmpty();
    }

    [Test]
    public async Task Search_FiltersByCreatedFrom()
    {
        var page = await BuildReader().SearchAsync(
            new MutationAuditCriteria { EntityType = _entityType, CreatedFrom = _digitalCreatedAt.AddMilliseconds(1) },
            CancellationToken.None
        );

        await Assert.That(page.Total).IsEqualTo(1);
        await Assert.That(page.Rows[0].Id).IsEqualTo(_cmsRowId);
    }
}