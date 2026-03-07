using Digital.Net.Core.Messages;
using Digital.Net.Entities.Models.Documents;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Core.Services.HttpContext;

public interface IHttpCacheService
{
    Result<FileResult> GetCachedDocument(Document? document, string? contentType = null);
}