using System.Linq.Expressions;
using SafariDigital.Core.Predicate;
using SafariDigital.Database.Models.UserTable;
using SafariDigital.Database.Repository;

namespace SafariDigital.Services.PaginationService.Services;

public class UserPaginationService(IRepository<User> repositoryService)
    : PaginationService<User, UserPublicModel, UserQuery>(repositoryService)
{
    protected override Expression<Func<User, bool>> Filter(UserQuery query)
    {
        var filter = PredicateBuilder.New<User>(true);
        if (!string.IsNullOrEmpty(query.Username))
            filter = filter.And(x => x.Username.StartsWith(query.Username));
        if (!string.IsNullOrEmpty(query.Email))
            filter = filter.And(x => x.Email.StartsWith(query.Email));
        if (query.Role.HasValue)
            filter = filter.And(x => x.Role == query.Role);
        if (query.IsActive.HasValue)
            filter = filter.And(x => x.IsActive == query.IsActive);
        if (query.CreatedAt.HasValue)
            filter = filter.And(x => x.CreatedAt >= query.CreatedAt);
        if (query.UpdatedAt.HasValue)
            filter = filter.And(x => x.UpdatedAt >= query.UpdatedAt);
        return filter;
    }
}