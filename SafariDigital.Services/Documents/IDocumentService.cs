using Microsoft.AspNetCore.Http;
using Safari.Net.Core.Messages;
using SafariDigital.Data.Models.Database;

namespace SafariDigital.Services.Documents;

public interface IDocumentService
{
    Task<Result<Document>> SaveDocumentAsync(IFormFile file, EDocumentType type);
    Task<Result> RemoveDocumentAsync(Document? document);
    Task<Result> RemoveDocumentAsync(Guid id);
}