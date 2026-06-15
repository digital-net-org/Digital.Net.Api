using Digital.Net.Lib.Entities.Mutations;
using Digital.Net.Core.Http.Endpoints.Dto;
using Digital.Net.Core.Http.Services.Mutations;

namespace Digital.Net.Tests.Core.Http.Endpoints.Dto;

public class EntityMutationDtoTest
{
    private static MutationAuditRow BuildRow() => new()
    {
        Id = Guid.NewGuid(),
        CreatedAt = DateTime.UtcNow,
        ChangeType = (int)ChangeType.Updated,
        EntityType = "Page",
        EntityId = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        IpAddress = "203.0.113.7",
        UserAgent = "Test/1.0",
        Payload = "{}"
    };

    [Test]
    public async Task Ctor_IncludesAuthorMetadata_ForAdmins()
    {
        var row = BuildRow();
        var dto = new EntityMutationDto(row, true);

        await Assert.That(dto.UserId).IsEqualTo(row.UserId);
        await Assert.That(dto.IpAddress).IsEqualTo(row.IpAddress);
        await Assert.That(dto.UserAgent).IsEqualTo(row.UserAgent);
    }

    [Test]
    public async Task Ctor_MasksAuthorMetadata_ForNonAdmins()
    {
        var row = BuildRow();
        var dto = new EntityMutationDto(row, false);

        await Assert.That(dto.IpAddress).IsNull();
        await Assert.That(dto.UserAgent).IsNull();
        // Business content stays visible.
        await Assert.That(dto.EntityType).IsEqualTo(row.EntityType);
        await Assert.That(dto.Payload).IsEqualTo(row.Payload);
    }
}