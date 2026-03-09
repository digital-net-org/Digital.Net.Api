using Digital.Net.Api.Services.Documents.Exceptions;
using Digital.Net.Core.Exceptions.types;
using Digital.Net.Core.Messages;
using Digital.Net.Entities.Models.Documents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Api.Services.Documents;

public class DocumentCacheService(
    IHttpContextAccessor httpContextAccessor,
    IDocumentService documentService
) : IDocumentCacheService
{
    public Result<FileResult?> GetCachedDocumentFile(Document? document)
    {
        var result = new Result<FileResult?>();
        if (document is null)
            return result.AddError(new DocumentNotFoundException());

        var etag = $"\"{document.Id}\"";

        var context = httpContextAccessor.HttpContext;
        if (context is null)
            return result.AddError(new UnhandledInternalError());

        var ifNoneMatch = context.Request.Headers.IfNoneMatch;
        if (ifNoneMatch.Count > 0 && ifNoneMatch.Any(v => v is not null && v.Contains(etag)))
        {
            context.Response.Headers.CacheControl = "public, max-age=0, must-revalidate";
            context.Response.Headers.ETag = etag;
            return result;
        }

        var fileResult = documentService.GetDocumentFile(document.Id, document.MimeType);
        if (fileResult.HasError || fileResult.Value is null)
            return result.AddError(new DocumentNotFoundException());

        context.Response.Headers.CacheControl = "public, max-age=0, must-revalidate";
        context.Response.Headers.ETag = etag;
        context.Response.Headers.Remove("Content-Disposition");

        result.Value = fileResult.Value;
        return result;
    }
}