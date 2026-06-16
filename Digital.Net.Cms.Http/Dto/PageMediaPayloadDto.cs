using Digital.Net.Cms.Models.Pages;

namespace Digital.Net.Cms.Http.Dto;

public class PageMediaPayloadDto : MediaPivotPayloadDto<PageMediaPayloadDto, PageMedia>
{
    public override void ApplyToPivot(PageMedia pivot) => pivot.Label = Label.Trim();
}
