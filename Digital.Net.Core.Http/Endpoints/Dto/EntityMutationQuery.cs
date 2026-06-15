using Digital.Net.Lib.Entities.Mutations;
using Digital.Net.Core.Http.Services.Pagination;

namespace Digital.Net.Core.Http.Endpoints.Dto;

public class EntityMutationQuery : Query
{
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public Guid? UserId { get; set; }
    public ChangeType? ChangeType { get; set; }
}