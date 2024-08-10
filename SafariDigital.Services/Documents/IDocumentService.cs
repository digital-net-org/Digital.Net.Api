using Microsoft.AspNetCore.Http;
using Safari.Net.Core.Messages;
using SafariDigital.Data.Models.Database;

namespace SafariDigital.Services.Documents;

public interface IDocumentService
{
    Task<Result<Document>> SaveDocumentAsync(IFormFile file, EDocumentType type);
}