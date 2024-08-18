using System.Linq.Expressions;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Safari.Net.Core.Predicates;
using Safari.Net.Data.Entities;
using Safari.Net.Data.Repositories;
using SafariDigital.Core;
using SafariDigital.Data.Models.Database;

namespace SafariDigital.Data.Services;

public class UserEntityService(IRepository<User> userRepository) : EntityService<User, UserQuery>(userRepository)
{
    private readonly IRepository<User> _userRepository = userRepository;
    private static bool ValidateUsername(string? str) => RegularExpressions.GetUsernameRegex().IsMatch(str ?? "");
    private static bool ValidateEmail(string? str) => RegularExpressions.GetEmailRegex().IsMatch(str ?? "");

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

    protected override void ValidatePatch(Operation<User> patch, User entity)
    {
        switch (patch.path)
        {
            case "/role" or "/password" or "/is_active" or "/avatar_id":
                throw new InvalidOperationException("This value cannot be patched");
            case "/username" when !ValidateUsername(patch.value.ToString()):
                throw new InvalidOperationException("Username does not meet requirements");
            case "/username" when _userRepository.Get(u => u.Username == patch.value.ToString()).Any():
                throw new InvalidOperationException("Username is already taken");
            case "/email" when !ValidateEmail(patch.value.ToString()):
                throw new InvalidOperationException("Email does not meet requirements");
            case "/email" when _userRepository.Get(u => u.Email == patch.value.ToString()).Any():
                throw new InvalidOperationException("Email is already taken");
        }
    }
}