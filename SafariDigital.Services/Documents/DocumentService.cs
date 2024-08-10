using Microsoft.AspNetCore.Http;
using Safari.Net.Core.Extensions.FormFileUtilities;
using Safari.Net.Core.Messages;
using Safari.Net.Data.Repositories;
using SafariDigital.Data.Models.Database;
using SafariDigital.Services.HttpContext;

namespace SafariDigital.Services.Documents;

public class DocumentService(IHttpContextService httpContextService, IRepository<Document> documentRepository) : IDocumentService
{
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
                Uploader = user
            };

            await documentRepository.CreateAsync(result.Value);
            await documentRepository.SaveAsync();
        }
        catch (Exception e)
        {
            result.AddError(e);
        }
        return result;
    }
}