using System.Linq.Expressions;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Safari.Net.Core.Messages;
using Safari.Net.Core.Predicates;
using Safari.Net.Data.Entities;
using Safari.Net.Data.Repositories;
using SafariDigital.Core.Application;
using SafariDigital.Data.Models.Database;

namespace SafariDigital.Data.Services;

public class UserService(IRepository<User> repositoryService)
    : EntityService<User, UserQuery>(repositoryService)
{
    protected override Expression<Func<User, bool>> Filter(UserQuery query)
    {
        var filter = PredicateBuilder.New<User>();
        if (!string.IsNullOrEmpty(query.Username))
            filter = filter.Add(x => x.Username.StartsWith(query.Username));
        if (!string.IsNullOrEmpty(query.Email))
            filter = filter.Add(x => x.Email.StartsWith(query.Email));
        if (query.Role.HasValue)
            filter = filter.Add(x => x.Role == query.Role);
        if (query.IsActive.HasValue)
            filter = filter.Add(x => x.IsActive == query.IsActive);
        if (query.CreatedAt.HasValue)
            filter = filter.Add(x => x.CreatedAt >= query.CreatedAt);
        if (query.UpdatedAt.HasValue)
            filter = filter.Add(x => x.UpdatedAt >= query.UpdatedAt);
        return filter;
    }

    protected override void ValidatePatch(Operation<User> patch, Result result)
    {
        if (patch.path is "/role" or "/password" or "/is_active")
            result.AddError(EApplicationMessage.UserPatchError);
    }
}