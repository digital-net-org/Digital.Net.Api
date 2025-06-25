using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Api.Entities.Models.Documents;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Entities.Models.Pages;

[Table("PageAsset"), Index(nameof(Path), IsUnique = true)]
public class PageAsset : Entity
{
    [Column("Path"), Required, MaxLength(2068)] // TODO: Unique with pages
    public required string Path { get; set; }
    
    [Column("DocumentId"), ForeignKey("Document"), Required]
    public Guid DocumentId { get; set; }

    public virtual Document? Document { get; set; }
}