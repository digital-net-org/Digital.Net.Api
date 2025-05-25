using Digital.Net.Api.Core.Exceptions;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Pages;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.Services.Pages.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Services.Pages.Validation.Generic;

public class PuckConfigValidationService(
    IRepository<PagePuckConfig, DigitalContext> puckConfigRepository
) : PageDocumentValidator<PagePuckConfig>(puckConfigRepository), IPuckConfigValidationService
{
    private readonly IRepository<PagePuckConfig, DigitalContext> _puckConfigRepository = puckConfigRepository;

    public override List<string> GetAllowedMimeTypes() => 
        ["application/javascript", "text/javascript", "application/x-javascript"];

    public Result ValidateUpload(IFormFile file, string version)
    {
        var result = base.ValidateUpload(file);
        if (_puckConfigRepository.Count(x => x.Version.ToLower() == version.ToLower()) > 0)
            result.AddError(new ResourceDuplicateException());
        return result;
    }

    public override Result<PagePuckConfig> ValidateDelete(int id)
    {
        var result = base.ValidateDelete(id);
        if (result.Value?.Pages.Count > 0)
        {
            result.AddError(new CannotDeletePublishedConfigException(result.Value.Id));
            result.Value = null;
        }
        return result;
    }
}