using Digital.Net.Cms.Context;
using Digital.Net.Cms.Models;
using Digital.Net.Core.Accessors;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Interceptors;
using Digital.Net.Core.Entities.Models.Documents;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.Entities.Models.Mutations;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Tests.Core.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Tests.Core.Entities.Interceptors;

public class MutationTrackingInterceptorTest : UnitTest
{
    [ClassDataSource<DatabaseFixture>]
    public required DatabaseFixture DbFixture { get; init; }

    private static readonly Guid StubUserId = Guid.NewGuid();
    private const string StubIp = "203.0.113.7";
    private const string StubUserAgent = "MutationTrackingTest/1.0";

    [Test]
    public async Task Create_TrackedEntity_PersistsCreatedMutation_WithOriginAndUser()
    {
        await using var ctx = CreateTrackedContext<DigitalContext>();
        var document = NewDocument();

        ctx.Add(document);
        await ctx.SaveChangesAsync();

        var mutation = await ReadMutation<DigitalContext>(document.Id);
        await Assert.That(mutation).IsNotNull();
        await Assert.That(mutation!.ChangeType).IsEqualTo(ChangeType.Created);
        await Assert.That(mutation.EntityType).IsEqualTo(nameof(Document));
        await Assert.That(mutation.UserId).IsEqualTo(StubUserId);
        await Assert.That(mutation.IpAddress).IsEqualTo(StubIp);
        await Assert.That(mutation.UserAgent).IsEqualTo(StubUserAgent);
    }

    [Test]
    public async Task Update_TrackedEntity_PersistsUpdatedMutation()
    {
        await using var ctx = CreateTrackedContext<DigitalContext>();
        var document = NewDocument();
        ctx.Add(document);
        await ctx.SaveChangesAsync();

        document.FileSize += 1;
        await ctx.SaveChangesAsync();

        var mutations = await ReadMutations<DigitalContext>(document.Id, ChangeType.Updated);
        await Assert.That(mutations.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Delete_TrackedEntity_PersistsDeletedMutation()
    {
        await using var ctx = CreateTrackedContext<DigitalContext>();
        var document = NewDocument();
        ctx.Add(document);
        await ctx.SaveChangesAsync();

        ctx.Remove(document);
        await ctx.SaveChangesAsync();

        var mutations = await ReadMutations<DigitalContext>(document.Id, ChangeType.Deleted);
        await Assert.That(mutations.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Save_UntrackedEntity_PersistsNoMutation()
    {
        await using var ctx = CreateTrackedContext<DigitalContext>();
        var @event = new Event { Name = $"no-loop-{TestId}" };

        ctx.Add(@event);
        await ctx.SaveChangesAsync();

        var mutation = await ReadMutation<DigitalContext>(@event.Id);
        await Assert.That(mutation).IsNull();
    }

    [Test]
    public async Task Create_TrackedEntity_InCmsContext_PersistsMutationInCmsSchema()
    {
        await using var ctx = CreateTrackedContext<CmsContext>();
        var tag = new Tag { Name = $"tag-{TestId}" };

        ctx.Add(tag);
        await ctx.SaveChangesAsync();

        var mutation = await ReadMutation<CmsContext>(tag.Id);
        await Assert.That(mutation).IsNotNull();
        await Assert.That(mutation!.ChangeType).IsEqualTo(ChangeType.Created);
        await Assert.That(mutation.EntityType).IsEqualTo(nameof(Tag));
    }

    private T CreateTrackedContext<T>() where T : DbContext
    {
        var provider = new ServiceCollection()
            .AddSingleton<IOriginAccessor>(new StubOriginAccessor(StubIp, StubUserAgent))
            .AddSingleton<IUserAccessor>(new StubUserAccessor(StubUserId))
            .BuildServiceProvider();

        var options = new DbContextOptionsBuilder<T>()
            .UseDigitalNpgsql<T>(DbFixture.Fixture.ConnectionString)
            .AddInterceptors(new MutationTrackingInterceptor(provider))
            .Options;

        return (T)Activator.CreateInstance(typeof(T), options)!;
    }

    private async Task<EntityMutation?> ReadMutation<T>(Guid entityId) where T : DbContext
    {
        await using var read = DbFixture.CreateContext<T>();
        return await read.Set<EntityMutation>().SingleOrDefaultAsync(m => m.EntityId == entityId);
    }

    private async Task<List<EntityMutation>> ReadMutations<T>(Guid entityId, ChangeType changeType) where T : DbContext
    {
        await using var read = DbFixture.CreateContext<T>();
        return await read.Set<EntityMutation>()
            .Where(m => m.EntityId == entityId && m.ChangeType == changeType)
            .ToListAsync();
    }

    private static Document NewDocument() => new()
    {
        FileName = $"mut-{Guid.NewGuid():N}.bin",
        MimeType = "application/octet-stream",
        FileSize = 1
    };

    private sealed class StubOriginAccessor(string? ip, string? userAgent) : IOriginAccessor
    {
        public RequestOrigin GetOrigin() => new(ip, userAgent);
        public RequestOrigin TryGetOrigin() => new(ip, userAgent);
    }

    private sealed class StubUserAccessor(Guid? userId) : IUserAccessor
    {
        public Guid GetUserId() => userId ?? throw new InvalidOperationException();
        public Guid? TryGetUserId() => userId;
        public User GetUser() => throw new NotSupportedException();
    }
}