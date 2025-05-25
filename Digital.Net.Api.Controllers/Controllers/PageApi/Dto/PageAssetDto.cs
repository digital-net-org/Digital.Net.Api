using Digital.Net.Api.Controllers.Controllers.UserApi.Dto;
using Digital.Net.Api.Entities.Models.Pages;

namespace Digital.Net.Api.Controllers.Controllers.PageApi.Dto;

public class PageAssetDto : PageAssetLightDto
{
    public PageAssetDto()
    {
    }

    public PageAssetDto(PageAsset pageAsset) : base(pageAsset)
    {
        FileName = pageAsset.Document?.FileName ?? "unknown";
        FileSize = pageAsset.Document?.FileSize ?? 0;
        
        var uploader = pageAsset.Document?.Uploader;
        if (uploader is not null)
            Uploader = new UserDto(uploader);
    }
    
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public UserDto? Uploader { get; set; }
}