using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Safari.Net.Core.Extensions.FormFileUtilities;
using Safari.Net.Core.Messages;
using Safari.Net.Data.Repositories;
using SafariDigital.Core.Application;
using SafariDigital.Data.Models.Database;
using SafariDigital.Services.HttpContext;

namespace SafariDigital.Services.Documents;

public class DocumentService(
    IConfiguration configuration,
    IHttpContextService httpContextService,
    IRepository<Document> documentRepository) : IDocumentService
{
    private string FileSystemPath => configuration.GetSection<string>(EApplicationSetting.FileSystemPath);

    public async Task<Result<Document>> SaveDocumentAsync(IFormFile file, EDocumentType type)
    {
        var result = new Result<Document>();
        try
        {
            var user = await httpContextService.GetAuthenticatedUser();
            result.Value = new Document
            {
                FileName = file.GenerateFileName(),
                DocumentType = type,
                MimeType = file.ContentType,
                FileSize = file.Length,
                UploaderId = user.Id
            };
            await documentRepository.CreateAsync(result.Value);
            result.WriteDocumentFile(file, Path.Combine(FileSystemPath, result.Value.FileName));
            await documentRepository.SaveAsync();
        }
        catch (Exception e)
        {
            result.AddError(e);
        }

        return result;
    }

    public async Task<Result> RemoveDocumentAsync(Document? document)
    {
        var result = new Result();
        try
        {
            if (document is null) throw new FileNotFoundException("This document could not be found in database.");
            DocumentUtils.RemoveDocumentFile(FileSystemPath, document);
            documentRepository.Delete(document);
            await documentRepository.SaveAsync();
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }

    public async Task<Result> RemoveDocumentAsync(Guid id)
    {
        var document = await documentRepository.GetByIdAsync(id);
        return await RemoveDocumentAsync(document);
    }
}