using Digital.Net.Api.Core.Exceptions;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Documents;
using Digital.Net.Api.Entities.Models.PuckConfigs;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.Services.Documents;
using Digital.Net.Api.Services.Views.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Api.Services.Views;

public class PuckConfigService(
    IRepository<PuckConfig, DigitalContext> puckConfigRepository,
    IPuckConfigValidationService puckConfigValidationService,
    IDocumentService documentService
) : IPuckConfigService
{
    public Result<PuckConfig> GetConfig(string version)
    {
        var result = new Result<PuckConfig>();
        var config = puckConfigRepository.Get(x => x.Version == version).FirstOrDefault();
        if (config is null)
            return result.AddError(new ResourceNotFoundException());
        
        result.Value = config;
        return result;
    }
    
    public Result<FileResult> GetConfigFile(PuckConfig config)
    {
        var result = new Result<FileResult>
        {
            Value = documentService.GetDocumentFile(config.DocumentId, "application/javascript")
        };
        
        if (result.Value is null)
            result.AddError(new NoPuckConfigFileException(config.Id));

        return result;
    }

    public async Task<Result<(PuckConfig, Document)>> UploadAsync(IFormFile file, string version, User? uploader)
    {
        var result = new Result<(PuckConfig, Document)>();
        if (result.Merge(await puckConfigValidationService.ValidateUpload(file, version)).HasError)
            return result;

        var documentResult = await documentService.SaveDocumentAsync(file, uploader);
        result.Merge(documentResult);

        if (result.HasError || documentResult.Value is null)
            return result;

        var config = new PuckConfig
        {
            DocumentId = documentResult.Value.Id,
            Version = version,
        };
        await puckConfigRepository.CreateAndSaveAsync(config);

        result.Value = (config, documentResult.Value);
        return result;
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var result = await puckConfigValidationService.GetDeletable(id);
        if (result.HasError)
            return result;
        try
        {
            await documentService.RemoveDocumentAsync(result.Value!.DocumentId);
            puckConfigRepository.Delete(result.Value);
            await puckConfigRepository.SaveAsync();
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }
        return result;
    }
}