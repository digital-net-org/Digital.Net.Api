using Digital.Pages.Data.Models.Views;

namespace Digital.Pages.Api.Dto.Entities;

public class ViewPublicModel
{
    public ViewPublicModel()
    {
    }

    public ViewPublicModel(View view)
    {
        Data = view?.Frame?.Data;
    }

    public string? Data { get; set; }
}