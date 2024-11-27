using Digital.Net.Core.Messages;
using Digital.Net.Core.Models;
using Digital.Net.Entities.Repositories;
using Digital.Net.Entities.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using SafariDigital.Core.Application;
using SafariDigital.Data.Models.Database.Frames;
using SafariDigital.Data.Models.Database.Views;
using SafariDigital.Data.Models.Dto.Frames;
using SafariDigital.Services.Frames.Models;

namespace SafariDigital.Services.Frames;

public class FrameService(
    IRepository<View> viewRepository,
    IRepository<Frame> frameRepository,
    IEntityService<Frame, FrameQuery> frameEntityService)
    : IFrameService
{
    public async Task<Result<FrameModel>> GetFrameAsync(int id)
    {
        var result = new Result<FrameModel>();
        var frame = await frameRepository.GetByIdAsync(id);
        if (frame is null) return result.AddError(EApplicationMessage.EntityNotFound);
        result.Value = Mapper.MapFromConstructor<Frame, FrameModel>(frame);
        return result;
    }

    public async Task<Result> DeleteFrameAsync(int id)
    {
        var result = new Result();
        var frame = await frameRepository.GetByIdAsync(id);
        if (frame is null) return result.AddError(EApplicationMessage.EntityNotFound);
        frameRepository.Delete(frame);
        await frameRepository.SaveAsync();
        return result;
    }

    public async Task<Result<FrameModel>> PatchFrameAsync(int id, JsonPatchDocument<Frame> patch)
    {
        var frame = await frameRepository.GetByIdAsync(id);
        if (frame is null) return new Result<FrameModel>().AddError(EApplicationMessage.EntityNotFound);
        return await frameEntityService.Patch<FrameModel>(patch, id);
    }

    public async Task<Result> TryFrameDuplicateAsync(string name)
    {
        var result = new Result();
        var frame = await frameRepository.Get(vf => vf.Name == name).FirstOrDefaultAsync();
        if (frame is not null) result.AddError(EApplicationMessage.EntityUniqueViolation);
        return result;
    }

    public async Task<Result<FrameModel>> CreateFrameAsync(CreateFrameRequest request)
    {
        var result = new Result<FrameModel>();
        var frame = new Frame { Name = request.Name, Data = request.Data };
        try
        {
            await frameRepository.CreateAsync(frame);
            if (request.ViewId is not null)
            {
                var view = await viewRepository.GetByIdAsync(request.ViewId);
                if (view is null)
                    result.AddError(EApplicationMessage.LinkedEntityNotFound);
                else
                    view.Frame = frame;
            }

            await frameRepository.SaveAsync();
            result.Value = Mapper.MapFromConstructor<Frame, FrameModel>(frame);
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }
}