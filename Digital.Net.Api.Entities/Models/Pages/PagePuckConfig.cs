using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Api.Entities.Models.Documents;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Entities.Models.Pages;

[Table("PagePuckConfig"), Index(nameof(Version), IsUnique = true)]
public class PagePuckConfig : EntityId
{
    [Column("DocumentId"), ForeignKey("Document"), Required]
    public Guid DocumentId { get; set; }

    [Column("Version"), Required, MaxLength(24)]
    public string Version { get; set; }

    public virtual Document? Document { get; set; }

    public virtual List<Page> Pages { get; set; } = [];
}