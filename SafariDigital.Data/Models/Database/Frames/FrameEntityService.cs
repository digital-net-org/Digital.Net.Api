using System.Linq.Expressions;
using System.Text;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Safari.Net.Core.Predicates;
using Safari.Net.Data.Entities;
using Safari.Net.Data.Repositories;

namespace SafariDigital.Data.Models.Database.Frames;

public class FrameEntityService(IRepository<Frame> viewFrameRepository)
    : EntityService<Frame, FrameQuery>(viewFrameRepository)
{
    protected override Expression<Func<Frame, bool>> Filter(FrameQuery query)
    {
        var filter = PredicateBuilder.New<Frame>();
        if (!string.IsNullOrEmpty(query.Name))
            filter = filter.Add(x => x.Name.StartsWith(query.Name));
        if (query.CreatedAt.HasValue)
            filter = filter.Add(x => x.CreatedAt >= query.CreatedAt);
        if (query.UpdatedAt.HasValue)
            filter = filter.Add(x => x.UpdatedAt >= query.UpdatedAt);
        return filter;
    }

    protected override void ValidatePatch(Operation<Frame> patch, Frame entity)
    {
        switch (patch.path)
        {
            case "name" when patch.value.ToString()?.Length > 1024:
                throw new InvalidOperationException("Name maximum length exceeded");
            case "data":
            {
                var value = patch.value.ToString();
                if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                    throw new InvalidOperationException("Data cannot be empty, should be a valid base64 string");

                patch.value = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
                break;
            }
        }
    }
}