using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Pages;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.Services.Documents;
using Digital.Net.Api.Services.Pages.Validation.Generic;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Services.Pages;

public class PageAssetService(
    IDocumentService documentService,
    IPageAssetValidationService pageAssetValidationService,
    IRepository<PageAsset, DigitalContext> pageAssetRepository
) : IPageAssetService
{
    public async Task<Result> UploadAsync(IFormFile file, string path, User? uploader)
    {
        var result = pageAssetValidationService.ValidateUpload(file, path);
        if (result.HasError)
            return result;

        var document = await result.TryAsync(async () => await documentService.SaveDocumentAsync(file, uploader));
        if (result.HasError || document is null)
            return result;

        await pageAssetRepository.CreateAndSaveAsync(
            new PageAsset()
            {
                DocumentId = document.Id,
                Path = path
            }
        );
        return result;
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var result = pageAssetValidationService.ValidateDelete(id);
        if (result.HasError)
            return result;
        try
        {
            await documentService.RemoveDocumentAsync(result.Value!.DocumentId);
            pageAssetRepository.Delete(result.Value);
            await pageAssetRepository.SaveAsync();
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }
        return result;
    }
}