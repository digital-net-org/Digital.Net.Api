using Safari.Net.Core.Messages;
using SafariDigital.Data.Models.Dto.Views;
using SafariDigital.Services.Views.Models;

namespace SafariDigital.Services.Views;

public interface IViewService
{
    Task<Result<ViewModel>> CreateViewAsync(CreateViewRequest payload);
    Task<Result> DeleteViewAsync(int id);
    Task<Result> TryViewDuplicateAsync(string title);
}