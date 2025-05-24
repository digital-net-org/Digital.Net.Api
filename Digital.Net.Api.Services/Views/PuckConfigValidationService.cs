using Digital.Net.Api.Core.Exceptions;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.PuckConfigs;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.Services.Views.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Services.Views;

public class PuckConfigValidationService(
    IRepository<PuckConfig, DigitalContext> puckConfigRepository
) : IPuckConfigValidationService
{
    private static readonly List<string> AllowedMimeTypes =
        ["application/javascript", "text/javascript", "application/x-javascript"];

    public async Task<Result> ValidateUpload(IFormFile file, string version)
    {
        var result = new Result();
        if (await puckConfigRepository.CountAsync(x => x.Version.ToLower() == version.ToLower()) > 0)
            return result.AddError(new ResourceDuplicateException());
        if (file.Length == 0)
            return result.AddError(new ResourceMalformedException());
        if (!AllowedMimeTypes.Contains(file.ContentType))
            return result.AddError(new ResourceContentTypeException());
        if (string.IsNullOrEmpty(file.FileName))
            return result.AddError(new ResourceMalformedException());
        return result;
    }

    public async Task<Result<PuckConfig>> GetDeletable(int id)
    {
        var result = new Result<PuckConfig>();
        var config = await puckConfigRepository.GetByIdAsync(id);
        if (config is null)
            return result.AddError(new ResourceNotFoundException());
        if (config.Views.Count > 0)
            return result.AddError(new CannotDeletePublishedConfigException(config.Id));
        result.Value = config;
        return result;
    }
}