using Digital.Net.Api.Core.Exceptions;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Pages;
using Digital.Net.Api.Entities.Repositories;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Services.Pages.Validation;

public class PageAssetValidationService(
    IRepository<PageAsset, DigitalContext> pageAssetRepository
) : PageDocumentValidator<PageAsset>(pageAssetRepository), IPageAssetValidationService
{
    private readonly IRepository<PageAsset, DigitalContext> _pageAssetRepository = pageAssetRepository;

    public override List<string> GetAllowedMimeTypes() =>
    [
        "image/png",
        "image/jpeg",
        "image/jpg",
        "image/gif",
        "image/webp",
        "image/svg+xml",
        "text/css"
    ];

    public Result ValidateUpload(IFormFile file, string path)
    {
        var result = base.ValidateUpload(file);
        if (_pageAssetRepository.Count(x => x.Path.ToLower() == path.ToLower()) > 0)
            result.AddError(new ResourceDuplicateException());
        return result;
    }
}