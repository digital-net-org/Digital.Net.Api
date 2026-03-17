using System.ComponentModel.DataAnnotations.Schema;

namespace Digital.Net.Cms.Models;

[Table("PageSheet")]
public class PageSheet
{
    [Column("PageId")]
    public Guid PageId { get; set; }

    [Column("SheetId")]
    public Guid SheetId { get; set; }

    [Column("LoadOrder")]
    public int LoadOrder { get; set; }

    public virtual Page Page { get; set; } = null!;

    public virtual Sheet Sheet { get; set; } = null!;
}
