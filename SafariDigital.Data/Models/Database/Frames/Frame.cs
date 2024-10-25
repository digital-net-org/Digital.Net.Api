using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Safari.Net.Data.Entities.Models;
using SafariDigital.Data.Models.Database.Views;

namespace SafariDigital.Data.Models.Database.Frames;

[Table("frame"), Index(nameof(Name), IsUnique = true)]
public class Frame : EntityWithId
{
    [Column("name"), Required, MaxLength(1024)]
    public required string Name { get; set; }

    public virtual View? View { get; set; }

    [Column("data")]
    public string? Data { get; set; }
}