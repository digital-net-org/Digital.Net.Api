using Digital.Net.Core.Services.Documents.Exceptions;
using Digital.Net.Core.Services.Documents.Extensions;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Documents;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Lib.Configuration;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Lib.Messages;
using Digital.Net.Lib.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Digital.Net.Core.Services.Documents;

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

        if (DocumentTypes.SvgMimeTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            file = await SvgSanitizer.SanitizeAsync(file);

        result.Value = new Document(uploader, file);
        await context.Documents.AddAsync(result.Value);

        var fullPath = Path.GetFullPath(GetDocumentPath(result.Value));
        var dir = Path.GetDirectoryName(fullPath);
        if (dir is not null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        await context.SaveChangesAsync();
        return result;
    }
}