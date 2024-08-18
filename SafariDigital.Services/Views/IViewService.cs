using Safari.Net.Core.Messages;
using SafariDigital.Data.Models.Dto.Views;
using SafariDigital.Services.Views.Models;

namespace SafariDigital.Services.Views;

public interface IViewService
{
    Task<Result<ViewModel>> CreateAsync(CreateViewRequest payload);
}