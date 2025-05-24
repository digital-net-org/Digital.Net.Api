using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Models.Documents;
using Digital.Net.Api.Entities.Models.PuckConfigs;
using Digital.Net.Api.Entities.Models.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Api.Services.Views;

public interface IPuckConfigService
{
    Result<PuckConfig> GetConfig(string version);
    Result<FileResult> GetConfigFile(PuckConfig config);
    Task<Result<(PuckConfig, Document)>> UploadAsync(IFormFile file, string version, User? uploader);
    Task<Result> DeleteAsync(int id);
}