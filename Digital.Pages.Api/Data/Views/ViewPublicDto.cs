namespace Digital.Pages.Api.Data.Views;

public class ViewPublicDto
{
    public ViewPublicDto()
    {
    }

    public ViewPublicDto(View view)
    {
        Data = view.Frame?.Data;
    }

    public string? Data { get; set; }
}