using System.Linq.Expressions;
using Digital.Net.Core.Predicates;
using Digital.Net.Entities.Repositories;
using Digital.Net.Entities.Services;

namespace SafariDigital.Data.Models.Database.Frames;

public class FrameEntityService(IRepository<Frame> viewFrameRepository)
    : EntityService<Frame, FrameQuery>(viewFrameRepository)
{
    protected override Expression<Func<Frame, bool>> Filter(Expression<Func<Frame, bool>> predicate, FrameQuery query)
    {
        if (!string.IsNullOrEmpty(query.Name))
            predicate = predicate.Add(x => x.Name.StartsWith(query.Name));
        return predicate;
    }
}