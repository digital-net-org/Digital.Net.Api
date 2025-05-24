using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Models.PuckConfigs;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Services.Views;

public interface IPuckConfigValidationService
{
    public Task<Result> ValidateUpload(IFormFile file, string version);
    public Task<Result<PuckConfig>> GetDeletable(int id);
}