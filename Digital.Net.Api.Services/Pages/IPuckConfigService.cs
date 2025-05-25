using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Models.Users;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Services.Pages;

public interface IPuckConfigService
{
    Task<Result> UploadAsync(IFormFile file, string version, User? uploader);
    Task<Result> DeleteAsync(int id);
}