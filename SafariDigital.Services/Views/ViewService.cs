using Digital.Net.Core.Messages;
using Digital.Net.Core.Models;
using Digital.Net.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using SafariDigital.Core.Application;
using SafariDigital.Data.Models.Database.Views;
using SafariDigital.Data.Models.Dto.Views;
using SafariDigital.Services.Views.Models;

namespace SafariDigital.Services.Views;

public class ViewService(IRepository<View> viewRepository) : IViewService
{
    public async Task<Result<ViewModel>> CreateViewAsync(CreateViewRequest payload)
    {
        var result = new Result<ViewModel>();
        try
        {
            var view = new View { Title = payload.Title };
            await viewRepository.CreateAsync(view);
            await viewRepository.SaveAsync();
            result.Value = Mapper.MapFromConstructor<View, ViewModel>(view);
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

    public async Task<Result> TryViewDuplicateAsync(string title)
    {
        var result = new Result();
        var view = await viewRepository.Get(v => v.Title == title).FirstOrDefaultAsync();
        if (view is not null) result.AddError(EApplicationMessage.EntityUniqueViolation);
        return result;
    }
}