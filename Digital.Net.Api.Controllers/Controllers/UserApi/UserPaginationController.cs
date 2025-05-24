using System.Linq.Expressions;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.Controllers.Controllers.UserApi.Dto;
using Digital.Net.Api.Controllers.Generic.Pagination;
using Digital.Net.Api.Core.Predicates;
using Digital.Net.Api.Services.Authentication.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Api.Controllers.Controllers.UserApi;

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