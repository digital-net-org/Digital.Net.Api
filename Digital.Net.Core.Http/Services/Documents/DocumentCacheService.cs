using Digital.Net.Core.Entities.Models.Documents;
using Digital.Net.Core.Services.Documents;
using Digital.Net.Core.Services.Documents.Exceptions;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Lib.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Digital.Net.Core.Http.Services.Documents;

public class DocumentCacheService(
    IHttpContextAccessor httpContextAccessor,
    IDocumentService documentService
)
{
    public Result<IResult?> GetCachedDocumentFile(Document? document)
    {
        var result = new Result<IResult?>();
        if (document is null)
            return result.AddError(new DocumentNotFoundException());

        var context = httpContextAccessor.HttpContext;
        if (context is null)
            return result.AddError(new UnhandledInternalError());

        var path = documentService.ResolveExistingPath(document);
        if (path is null)
            return result.AddError(new DocumentNotFoundException());

        context.Response.Headers.CacheControl = "public, max-age=0, must-revalidate";

        result.Value = Results.File(
            path,
            document.MimeType,
            lastModified: document.UpdatedAt ?? document.CreatedAt,
            entityTag: new EntityTagHeaderValue($"\"{document.Id}\""),
            enableRangeProcessing: true
        );
        return result;
    }
}
