using System.Linq.Expressions;
using Digital.Net.Core.Predicates;
using Digital.Net.Entities.Repositories;
using Digital.Net.Entities.Services;

namespace SafariDigital.Data.Models.Database.Users;

public class UserEntityService(IRepository<User> userRepository) : EntityService<User, UserQuery>(userRepository)
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