using System.Linq.Expressions;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Safari.Net.Core.Predicates;
using Safari.Net.Data.Entities;
using Safari.Net.Data.Repositories;
using SafariDigital.Data.Models.Database.Frames;

namespace SafariDigital.Data.Services;

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
            case "data" when string.IsNullOrEmpty(patch.value.ToString())
                             || string.IsNullOrWhiteSpace(patch.value.ToString()):
                throw new InvalidOperationException("Data cannot be empty, should be a valid JSON string");
            case "data" when patch.value.ToString() is not null:
                try
                {
                    JObject.Parse(patch.value.ToString()!);
                }
                catch (JsonReaderException)
                {
                    throw new InvalidOperationException("Data should be a valid JSON string");
                }

                break;
        }
    }
}