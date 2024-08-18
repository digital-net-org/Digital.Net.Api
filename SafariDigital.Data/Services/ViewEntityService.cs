using System.Linq.Expressions;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Safari.Net.Core.Predicates;
using Safari.Net.Data.Entities;
using Safari.Net.Data.Repositories;
using SafariDigital.Core;
using SafariDigital.Data.Models.Database;

namespace SafariDigital.Data.Services;

public class ViewEntityService(IRepository<View> viewRepository, IRepository<ViewFrame> viewFrameRepository)
    : EntityService<View, ViewQuery>(viewRepository)
{
    private readonly IRepository<ViewFrame> _viewFrameRepository = viewFrameRepository;
    private readonly IRepository<View> _viewRepository = viewRepository;
    private static bool ValidateUsername(string? str) => RegularExpressions.GetUsernameRegex().IsMatch(str ?? "");
    private static bool ValidateEmail(string? str) => RegularExpressions.GetEmailRegex().IsMatch(str ?? "");

    protected override Expression<Func<View, bool>> Filter(ViewQuery query)
    {
        var filter = PredicateBuilder.New<View>();
        if (!string.IsNullOrEmpty(query.Title))
            filter = filter.Add(x => x.Title.StartsWith(query.Title));
        if (query.Type.HasValue)
            filter = filter.Add(x => x.Type == query.Type);
        if (query.IsPublished.HasValue)
            filter = filter.Add(x => x.IsPublished == query.IsPublished);
        if (query.CreatedAt.HasValue)
            filter = filter.Add(x => x.CreatedAt >= query.CreatedAt);
        if (query.UpdatedAt.HasValue)
            filter = filter.Add(x => x.UpdatedAt >= query.UpdatedAt);
        return filter;
    }

    protected override void ValidatePatch(Operation<View> patch, View entity)
    {
        switch (patch.path)
        {
            case "/type":
                throw new InvalidOperationException("This value cannot be patched");
            case "/title" when patch.value.ToString()?.Length > 1024:
                throw new InvalidOperationException("Title maximum length exceeded");
            case "/title" when string.IsNullOrEmpty(patch.value.ToString())
                               || string.IsNullOrWhiteSpace(patch.value.ToString()):
                throw new InvalidOperationException("Title cannot be empty");
            case "/title" when _viewRepository.Get(v => v.Title == patch.value.ToString()).Any(v => v.Id != entity.Id):
                throw new InvalidOperationException("Title already exists");
            case "/published_frame_id" when _viewFrameRepository
                .Get(v => v.Id == (int)patch.value)
                .Any(vf => vf.ViewId != entity.Id):
                throw new InvalidOperationException("Frame does not belong to this view");
        }
    }
}