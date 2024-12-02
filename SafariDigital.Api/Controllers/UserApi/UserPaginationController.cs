using System.Linq.Expressions;
using Digital.Net.Core.Predicates;
using Digital.Net.Entities.Repositories;
using Digital.Net.Mvc.Controllers.Pagination;
using Microsoft.AspNetCore.Mvc;
using SafariDigital.Api.Attributes;
using SafariDigital.Api.Controllers.UserApi.Dto;
using SafariDigital.Data.Models.Database.Users;

namespace SafariDigital.Api.Controllers.UserApi;

[ApiController, Route("user"), Authorize(Role = EUserRole.User)]
public class UserPaginationController(
    IRepository<User> userRepository
) : PaginationController<User, UserModel, UserQuery>(userRepository)
{
    protected override Expression<Func<User, bool>> Filter(Expression<Func<User, bool>> predicate, UserQuery query)
    {
        if (!string.IsNullOrEmpty(query.Username))
            predicate = predicate.Add(x => x.Username.StartsWith(query.Username));
        if (!string.IsNullOrEmpty(query.Email))
            predicate = predicate.Add(x => x.Email.StartsWith(query.Email));
        if (query.Role.HasValue)
            predicate = predicate.Add(x => x.Role == query.Role);
        if (query.IsActive.HasValue)
            predicate = predicate.Add(x => x.IsActive == query.IsActive);
        return predicate;
    }
}