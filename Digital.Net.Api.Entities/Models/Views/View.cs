using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Api.Entities.Attributes;
using Digital.Net.Api.Entities.Models.Pages;
using Digital.Net.Api.Entities.Models.PuckConfigs;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Entities.Models.Views;

[Table("View"), Index(nameof(Name), IsUnique = true)]
public class View : EntityGuid
{
    [Column("Name"), Required, MaxLength(1024)]
    public required string Name { get; set; }

    [Column("Data"), DataFlag("json")]
    public string? Data { get; set; }

    [Column("PuckConfigId"), Required, ForeignKey("PuckConfig")]
    public required int PuckConfigId { get; set; }

    public virtual List<Page> Pages { get; set; } = [];
    public virtual required PuckConfig PuckConfig { get; set; }
}