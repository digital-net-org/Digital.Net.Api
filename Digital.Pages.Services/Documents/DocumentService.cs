using System.Security.Authentication;
using Digital.Lib.Net.Authentication.Services.Authentication.ApiUsers;
using Digital.Lib.Net.Core.Extensions.FormFileUtilities;
using Digital.Lib.Net.Core.Messages;
using Digital.Lib.Net.Core.Random;
using Digital.Lib.Net.Entities.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Digital.Pages.Core.Application;
using Digital.Pages.Data.Models.Documents;
using Digital.Pages.Data.Models.Users;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Digital.Pages.Services.Documents;

public class DocumentService(
    IConfiguration configuration,
    IApiUserService<User> apiUserService,
    IRepository<Document> documentRepository
) : IDocumentService
{
    private string FileSystemPath => configuration.GetSection<string>(ApplicationSettingPath.FileSystemPath);

    public async Task<Result<Document>> SaveImageDocumentAsync(IFormFile form, int? quality = null)
    {
        try
        {
            await using var ms = form.OpenReadStream();
            using var image = await Image.LoadAsync(ms);
            using var memStream = new MemoryStream();

            var encoder = new JpegEncoder { Quality = quality ?? 75 };
            var fileName =
                $"{Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 60)}.jpg";
            await image.SaveAsync(memStream, encoder);

            var compressed = new FormFile(memStream, 0, memStream.ToArray().Length, "avatar", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var result = await SaveDocumentAsync(compressed);
            result.Merge(await compressed.TryWriteFileAsync(Path.Combine(FileSystemPath, fileName)));
            return result;
        }
        catch (Exception ex)
        {
            return new Result<Document>().AddError(ex);
        }
    }

    public async Task<Result> RemoveDocumentAsync(Document? document)
    {
        var result = new Result();
        try
        {
            if (document is null) throw new FileNotFoundException("This document could not be found in database.");
            DocumentUtils.RemoveDocumentFile(FileSystemPath, document);
            documentRepository.Delete(document);
            await documentRepository.SaveAsync();
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }

    public async Task<Result> RemoveDocumentAsync(Guid id)
    {
        var document = await documentRepository.GetByIdAsync(id);
        return await RemoveDocumentAsync(document);
    }

    public Result WriteDocument(IFormFile form, Document document) =>
        form.TryWriteFile(Path.Combine(FileSystemPath, document.FileName));

    private async Task<Result<Document>> SaveDocumentAsync(IFormFile file)
    {
        var result = new Result<Document>();
        try
        {
            var user = await apiUserService.GetAuthenticatedUserAsync() ?? throw new AuthenticationException();
            result.Value = new Document
            {
                FileName = file.FileName,
                MimeType = file.ContentType,
                FileSize = file.Length,
                UploaderId = user.Id
            };
            await documentRepository.CreateAsync(result.Value);
            await documentRepository.SaveAsync();
        }
        catch (Exception e)
        {
            result.AddError(e);
        }

        return result;
    }
}