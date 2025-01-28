using SafariDigital.Data.Models.Views;

namespace SafariDigital.Api.Dto.Entities;

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