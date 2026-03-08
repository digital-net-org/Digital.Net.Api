using Digital.Net.Api.Services.Documents.Exceptions;
using Digital.Net.Core.Configuration;
using Digital.Net.Core.Exceptions.types;
using Digital.Net.Core.Messages;
using Digital.Net.Api.Services.Documents.Extensions;
using Digital.Net.Core.Settings;
using Digital.Net.Entities.Context;
using Digital.Net.Entities.Models.Documents;
using Digital.Net.Entities.Models.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Digital.Net.Api.Services.Documents;

public class DocumentService(
    DigitalContext context,
    IConfiguration configuration
) : IDocumentService
{
    public string GetDocumentPath(Document document) => Path.Combine(
        configuration.Get<string>(AppSettings.FileSystemPathKey) ?? AppSettings.DefaultFileSystemPath, 
        document.FileName
    );

    public Result<FileResult?> GetDocumentFile(Guid documentId, string? contentType = null)
    {
        var result = new Result<FileResult?>();
        var document = context.Documents.Find(documentId);
        if (document is null)
            return result.AddError(new ResourceNotFoundException());
        var path = GetDocumentPath(document);
        if (!File.Exists(path))
            return result.AddError(new ResourceNotFoundException());

        var fileBytes = File.ReadAllBytes(path);
        result.Value = new FileContentResult(fileBytes, contentType ?? "application/octet-stream")
        {
            FileDownloadName = document.FileName,
            LastModified = document.UpdatedAt ?? document.CreatedAt,
        };
        if (result.Value is null)
            result.AddError(new ResourceNotFoundException());

        return result;
    }

    /// TODO: To move in a controller
    // public Result<FileResult> GetCachedDocumentFile(Document? document, string? contentType = null)
    // {
    //     var result = new Result<FileResult>();
    //     var etag = document?.Id.ToString();
    //
    //     if (result.HasError || document is null)
    //         return result.AddError(new DocumentNotFoundException());
    //     if (httpContextService.Request.Headers.TestIfNoneMatch(etag))
    //         return result;
    //
    //     var file = result.Try(() => GetDocumentFile(document.Id, contentType));
    //     if (result.HasError || file is null)
    //         return result.AddError(new DocumentNotFoundException());
    //
    //     httpContextService.Response.Headers.CacheControl = "public, max-age=0, must-revalidate";
    //     httpContextService.Response.Headers.ETag = etag;
    //     httpContextService.Response.Headers.Remove("Content-Disposition");
    //     result.Value = file;
    //     return result;
    // }

    public async Task<Result<Document>> SaveImageDocumentAsync(IFormFile form, User? uploader, int? quality = null)
    {
        var result = new Result<Document>();
        var compressed = await form.CompressImageAsync(quality: quality);
        if (compressed.HasError || compressed.Value is null)
            return result.Merge(compressed);

        result = await SaveDocumentAsync(compressed.Value, uploader);
        return result;
    }

    public async Task<Result> RemoveDocumentAsync(Guid id)
    {
        var document = await context.Documents.FindAsync(id);
        var result = new Result();
        if (document is null)
            return result.AddError(new DocumentNotFoundException());

        var path = GetDocumentPath(document);
        if (File.Exists(path))
            File.Delete(path);

        context.Documents.Remove(document);
        await context.SaveChangesAsync();
        return result;
    }

    public async Task<Result<Document>> SaveDocumentAsync(IFormFile file, User? uploader)
    {
        var result = new Result<Document>();
        if (uploader is null)
            return result.AddError(new NoUploaderException());

        result.Value = new Document(uploader, file);
        await context.Documents.AddAsync(result.Value);

        await using var stream = new FileStream(GetDocumentPath(result.Value), FileMode.Create);
        await file.CopyToAsync(stream);

        await context.SaveChangesAsync();
        return result;
    }
}