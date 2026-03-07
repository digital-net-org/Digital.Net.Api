using Digital.Net.Core.Messages;
using Digital.Net.Core.Services.Documents;
using Digital.Net.Core.Services.Documents.Exceptions;
using Digital.Net.Core.Services.HttpContext.Extensions;
using Digital.Net.Entities.Models.Documents;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Core.Services.HttpContext;

public class HttpCacheService(
    IDocumentService documentService,
    IHttpContextService httpContextService
) : IHttpCacheService
{
    public Result<FileResult> GetCachedDocument(Document? document, string? contentType = null)
    {
        var result = new Result<FileResult>();
        var etag = document?.Id.ToString();

        if (result.HasError || document is null)
            return result.AddError(new DocumentNotFoundException());
        if (httpContextService.Request.Headers.TestIfNoneMatch(etag))
            return result;
        
        var file = result.Try(() => documentService.GetDocumentFile(document.Id, contentType));
        if (result.HasError || file is null)
            return result.AddError(new DocumentNotFoundException());

        httpContextService.Response.Headers.CacheControl = "public, max-age=0, must-revalidate";
        httpContextService.Response.Headers.ETag = etag;
        httpContextService.Response.Headers.Remove("Content-Disposition");
        result.Value = file;
        return result;
    }
}