using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Models.Documents;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Api.Services.HttpContext;

public interface IHttpCacheService
{
    Result<FileResult> GetCachedDocument(Document? document, string? contentType = null);
}