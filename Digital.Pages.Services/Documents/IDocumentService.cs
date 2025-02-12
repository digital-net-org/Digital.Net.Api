using Digital.Lib.Net.Core.Messages;
using Microsoft.AspNetCore.Http;
using Digital.Pages.Data.Models.Documents;

namespace Digital.Pages.Services.Documents;

public interface IDocumentService
{
    Task<Result<Document>> SaveImageDocumentAsync(IFormFile file, int? quality = null);
    Task<Result> RemoveDocumentAsync(Document? document);
    Task<Result> RemoveDocumentAsync(Guid id);
}