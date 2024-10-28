using Microsoft.AspNetCore.JsonPatch;
using Safari.Net.Core.Messages;
using SafariDigital.Data.Models.Database.Frames;
using SafariDigital.Data.Models.Dto.Frames;
using SafariDigital.Services.Frames.Models;

namespace SafariDigital.Services.Frames;

public interface IFrameService
{
    Task<Result<FrameModel>> GetFrameAsync(int id);
    Task<Result> DeleteFrameAsync(int id);
    Task<Result<FrameModel>> PatchFrameAsync(int id, JsonPatchDocument<Frame> patch);
    Task<Result> TryFrameDuplicateAsync(string name);
    Task<Result<FrameModel>> CreateFrameAsync(CreateFrameRequest request);
}