using Digital.Net.Core.Entities.Models.Documents;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Lib.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Core.Services.Documents;

public interface IDocumentService
{
    string GetDocumentPath(Document document);
    Result<FileResult?> GetDocumentFile(Guid documentId, string? contentType = null);
    Task<Result<Document>> SaveDocumentAsync(IFormFile file, User? uploader);
    Task<Result<Document>> SaveImageDocumentAsync(IFormFile file, User? uploader, int? quality = null);
    Task<Result> RemoveDocumentAsync(Guid id);
}