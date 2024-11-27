using Digital.Net.Core.Messages;
using Microsoft.AspNetCore.Http;
using SafariDigital.Data.Models.Database.Documents;

namespace SafariDigital.Services.Documents;

public interface IDocumentService
{
    Task<Result<Document>> SaveImageDocumentAsync(IFormFile file, EDocumentType type, int? quality = null);
    Task<Result> RemoveDocumentAsync(Document? document);
    Task<Result> RemoveDocumentAsync(Guid id);
}