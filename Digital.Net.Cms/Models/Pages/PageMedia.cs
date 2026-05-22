using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Cms.Models.Medias;
using Digital.Net.Core.Entities.Attributes;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Lib.String;

namespace Digital.Net.Cms.Models.Pages;

[Table("PageMedia")]
[PivotResolution("/media", Ownership.Dissociate)]
public class PageMedia : Pivot<Page, Media>
{
    [Column("Label")]
    [MaxLength(64)]
    [RegexValidation(RegularExpressions.MediaLabelPattern)]
    public string? Label { get; set; }
}