using System.Linq.Expressions;
using Digital.Net.Core.Predicates;
using Digital.Net.Entities.Repositories;
using Digital.Net.Entities.Services;

namespace SafariDigital.Data.Models.Database.Views;

public class ViewEntityService(IRepository<View> viewRepository) : EntityService<View, ViewQuery>(viewRepository)
{
    protected override Expression<Func<View, bool>> Filter(Expression<Func<View, bool>> predicate, ViewQuery query)
    {
        if (!string.IsNullOrEmpty(query.Title))
            predicate = predicate.Add(x => x.Title.StartsWith(query.Title));
        if (query.IsPublished.HasValue)
            predicate = predicate.Add(x => x.IsPublished == query.IsPublished);
        return predicate;
    }
}