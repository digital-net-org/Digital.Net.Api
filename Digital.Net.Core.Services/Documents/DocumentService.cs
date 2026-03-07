using Digital.Net.Core.Configuration;
using Digital.Net.Core.Exceptions.types;
using Digital.Net.Core.Messages;
using Digital.Net.Core.Services.Documents.Exceptions;
using Digital.Net.Core.Services.Documents.Extensions;
using Digital.Net.Core.Settings;
using Digital.Net.Entities.Models.Documents;
using Digital.Net.Entities.Models.Users;
using Digital.Net.Entities.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Digital.Net.Core.Services.Documents;

public class DocumentService(
    IRepository<Document> documentRepository,
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
        var document = documentRepository.GetById(documentId);
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
        var document = await documentRepository.GetByIdAsync(id);
        var result = new Result();
        if (document is null)
            return result.AddError(new DocumentNotFoundException());

        var path = GetDocumentPath(document);
        if (File.Exists(path))
            File.Delete(path);

        documentRepository.Delete(document);
        await documentRepository.SaveAsync();
        return result;
    }

    public async Task<Result<Document>> SaveDocumentAsync(IFormFile file, User? uploader)
    {
        var result = new Result<Document>();
        if (uploader is null)
            return result.AddError(new NoUploaderException());

        result.Value = new Document(uploader, file);
        await documentRepository.CreateAsync(result.Value);

        await using var stream = new FileStream(GetDocumentPath(result.Value), FileMode.Create);
        await file.CopyToAsync(stream);

        await documentRepository.SaveAsync();
        return result;
    }
}