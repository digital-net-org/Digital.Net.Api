using Safari.Net.Core.Messages;
using Safari.Net.Core.Models;
using Safari.Net.Data.Repositories;
using SafariDigital.Data.Models.Database;
using SafariDigital.Data.Models.Dto.Views;
using SafariDigital.Services.Views.Models;

namespace SafariDigital.Services.Views;

public class ViewService(IRepository<View> viewRepository, IRepository<ViewFrame> viewFrameRepository)
    : IViewService
{
    public async Task<Result<ViewModel>> CreateAsync(CreateViewRequest payload)
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
}