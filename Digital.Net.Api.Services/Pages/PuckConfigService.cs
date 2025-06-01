using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Pages;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.Services.Documents;
using Digital.Net.Api.Services.Pages.Validation.Generic;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Services.Pages;

public class PuckConfigService(
    IRepository<PagePuckConfig, DigitalContext> puckConfigRepository,
    IPuckConfigValidationService puckConfigValidationService,
    IDocumentService documentService
) : IPuckConfigService
{
    public async Task<Result> UploadAsync(IFormFile file, string version, User? uploader)
    {
        var result = puckConfigValidationService.ValidateUpload(file, version);
        if (result.HasError)
            return result;

        var document = await result.TryAsync(async () => await documentService.SaveDocumentAsync(file, uploader));
        if (result.HasError || document is null)
            return result;

        await puckConfigRepository.CreateAndSaveAsync(
            new PagePuckConfig
            {
                DocumentId = document.Id,
                Version = version
            }
        );
        return result;
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var result = puckConfigValidationService.ValidateDelete(id);
        if (result.HasError)
            return result;
        try
        {
            puckConfigRepository.Delete(result.Value!);
            await documentService.RemoveDocumentAsync(result.Value!.DocumentId);
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }
        return result;
    }
}