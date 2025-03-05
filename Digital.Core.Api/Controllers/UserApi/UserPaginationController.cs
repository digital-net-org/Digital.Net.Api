using System.Linq.Expressions;
using Digital.Core.Api.Controllers.UserApi.Dto;
using Digital.Lib.Net.Authentication.Attributes;
using Digital.Lib.Net.Core.Predicates;
using Digital.Lib.Net.Entities.Context;
using Digital.Lib.Net.Entities.Models.Users;
using Digital.Lib.Net.Entities.Repositories;
using Digital.Lib.Net.Mvc.Controllers.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Core.Api.Controllers.UserApi;

[ApiController, Route("user"), Authorize(AuthorizeType.Any)]
public class UserPaginationController(
    IRepository<User, DigitalContext> userRepository
) : PaginationController<User, DigitalContext, UserDto, UserQuery>(userRepository)
{
    protected override Expression<Func<User, bool>> Filter(Expression<Func<User, bool>> predicate, UserQuery query)
    {
        if (!string.IsNullOrEmpty(query.Username))
            predicate = predicate.Add(x => x.Username.StartsWith(query.Username));
        if (!string.IsNullOrEmpty(query.Email))
            predicate = predicate.Add(x => x.Email.StartsWith(query.Email));
        if (query.IsActive.HasValue)
            predicate = predicate.Add(x => x.IsActive == query.IsActive);
        return predicate;
    }
}