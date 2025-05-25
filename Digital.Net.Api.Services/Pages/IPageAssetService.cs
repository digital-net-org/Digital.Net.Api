using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Models.Users;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Services.Pages;

public interface IPageAssetService
{
    Task<Result> UploadAsync(IFormFile file, string path, User? uploader);
    Task<Result> DeleteAsync(int id);
}