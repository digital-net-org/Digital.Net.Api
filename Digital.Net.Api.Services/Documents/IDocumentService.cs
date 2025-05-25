using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Models.Documents;
using Digital.Net.Api.Entities.Models.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Api.Services.Documents;

public interface IDocumentService
{
    string GetDocumentPath(Document document);
    Result<FileResult?> GetDocumentFile(Guid documentId, string? contentType = null);
    Task<Result<Document>> SaveDocumentAsync(IFormFile file, User? uploader);
    Task<Result<Document>> SaveImageDocumentAsync(IFormFile file, User? uploader, int? quality = null);
    Task<Result> RemoveDocumentAsync(Guid id);
}