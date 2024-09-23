using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Safari.Net.Core.Messages;
using Safari.Net.Core.Models;
using Safari.Net.Data.Entities;
using Safari.Net.Data.Repositories;
using SafariDigital.Core.Application;
using SafariDigital.Data.Models.Database;
using SafariDigital.Data.Models.Dto.Views;
using SafariDigital.Data.Services;
using SafariDigital.Services.Views.Models;

namespace SafariDigital.Services.Views;

public class ViewService(
    IRepository<View> viewRepository,
    IRepository<ViewFrame> viewFrameRepository,
    IEntityService<ViewFrame, ViewFrameQuery> viewFrameEntityService)
    : IViewService
{
    public async Task<Result<ViewModel>> CreateViewAsync(CreateViewRequest payload)
    {
        var result = new Result<ViewModel>();
        try
        {
            var view = new View { Title = payload.Title, Type = payload.Type };
            await viewRepository.CreateAsync(view);
            await viewRepository.SaveAsync();
            result.Value = Mapper.Map<View, ViewModel>(view);
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }

    public async Task<Result> DeleteViewAsync(int id)
    {
        var result = new Result();
        var view = await viewRepository.GetByIdAsync(id);
        if (view is null) return result.AddError(EApplicationMessage.EntityNotFound);
        viewRepository.Delete(view);
        await viewRepository.SaveAsync();
        return result;
    }

    public async Task<Result<FrameModel>> GetViewFrameAsync(int viewId, int viewFrameId)
    {
        var result = new Result<FrameModel>();
        var viewFrame = await viewFrameRepository
            .Get(ViewPredicates.ByViewAndFrameIds(viewId, viewFrameId))
            .FirstOrDefaultAsync();
        if (viewFrame is null) return result.AddError(EApplicationMessage.EntityNotFound);
        result.Value = Mapper.Map<ViewFrame, FrameModel>(viewFrame);
        return result;
    }

    public async Task<Result> DeleteViewFrameAsync(int viewId, int viewFrameId)
    {
        var result = new Result();
        var viewFrame = await viewFrameRepository
            .Get(ViewPredicates.ByViewAndFrameIds(viewId, viewFrameId))
            .FirstOrDefaultAsync();

        if (viewFrame is null) return result.AddError(EApplicationMessage.EntityNotFound);
        viewFrameRepository.Delete(viewFrame);
        await viewFrameRepository.SaveAsync();
        return result;
    }

    public async Task<Result<FrameModel>> PatchViewFrameAsync(int viewId, int viewFrameId,
        JsonPatchDocument<ViewFrame> patch)
    {
        var viewFrame = await viewFrameRepository
            .Get(ViewPredicates.ByViewAndFrameIds(viewId, viewFrameId))
            .FirstOrDefaultAsync();
        if (viewFrame is null) return new Result<FrameModel>().AddError(EApplicationMessage.EntityNotFound);

        return await viewFrameEntityService.Patch<FrameModel>(patch, viewFrameId);
    }

    public async Task<Result> TryViewDuplicateAsync(string title)
    {
        var result = new Result();
        var view = await viewRepository.Get(v => v.Title == title).FirstOrDefaultAsync();
        if (view is not null) result.AddError(EApplicationMessage.EntityUniqueViolation);
        return result;
    }

    public async Task<Result> TryViewFrameDuplicateAsync(string name)
    {
        var result = new Result();
        var viewFrame = await viewFrameRepository.Get(vf => vf.Name == name).FirstOrDefaultAsync();
        if (viewFrame is not null) result.AddError(EApplicationMessage.EntityUniqueViolation);
        return result;
    }

    public async Task<Result<FrameModel>> CreateViewFrameAsync(int viewId, string name)
    {
        var result = new Result<FrameModel>();
        var viewFrame = new ViewFrame { Name = name, ViewId = viewId };
        try
        {
            await viewFrameRepository.CreateAsync(viewFrame);
            await viewFrameRepository.SaveAsync();
            result.Value = Mapper.Map<ViewFrame, FrameModel>(viewFrame);
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }
}