using System.Linq.Expressions;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Safari.Net.Core.Predicates;
using Safari.Net.Data.Entities;
using Safari.Net.Data.Repositories;
using SafariDigital.Data.Models.Database;

namespace SafariDigital.Data.Services;

public class ViewFrameEntityService(IRepository<ViewFrame> viewFrameRepository)
    : EntityService<ViewFrame, ViewFrameQuery>(viewFrameRepository)
{
    protected override Expression<Func<ViewFrame, bool>> Filter(ViewFrameQuery query)
    {
        var filter = PredicateBuilder.New<ViewFrame>();
        if (!string.IsNullOrEmpty(query.Name))
            filter = filter.Add(x => x.Name.StartsWith(query.Name));
        if (query.ViewId.HasValue)
            filter = filter.Add(x => x.ViewId == query.ViewId);
        if (query.CreatedAt.HasValue)
            filter = filter.Add(x => x.CreatedAt >= query.CreatedAt);
        if (query.UpdatedAt.HasValue)
            filter = filter.Add(x => x.UpdatedAt >= query.UpdatedAt);
        return filter;
    }

    protected override void ValidatePatch(Operation<ViewFrame> patch, ViewFrame entity)
    {
        switch (patch.path)
        {
            case "view_id":
                throw new InvalidOperationException("This value cannot be patched");
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