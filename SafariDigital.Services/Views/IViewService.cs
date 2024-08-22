using Microsoft.AspNetCore.JsonPatch;
using Safari.Net.Core.Messages;
using SafariDigital.Data.Models.Database;
using SafariDigital.Data.Models.Dto.Views;
using SafariDigital.Services.Views.Models;

namespace SafariDigital.Services.Views;

public interface IViewService
{
    Task<Result<ViewModel>> CreateViewAsync(CreateViewRequest payload);
    Task<Result> DeleteViewAsync(int id);
    Task<Result<FrameModel>> GetViewFrameAsync(int viewId, int viewFrameId);
    Task<Result<FrameModel>> CreateViewFrameAsync(int viewId, string name);
    Task<Result> DeleteViewFrameAsync(int viewId, int viewFrameId);
    Task<Result<FrameModel>> PatchViewFrameAsync(int viewId, int viewFrameId, JsonPatchDocument<ViewFrame> patch);
    Task<Result> TryViewDuplicateAsync(string title);
    Task<Result> TryViewFrameDuplicateAsync(string name);
}