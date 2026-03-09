using Digital.Net.Core.Messages;
using Digital.Net.Entities.Models.Documents;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Api.Services.Documents;

public interface IDocumentCacheService
{
    Result<FileResult?> GetCachedDocumentFile(Document? document);
}
