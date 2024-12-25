using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Digital.Net.Entities.Models;
using Microsoft.EntityFrameworkCore;
using SafariDigital.Data.Models.Views;

namespace SafariDigital.Data.Models.Frames;

[Table("Frame"), Index(nameof(Name), IsUnique = true)]
public class Frame : EntityGuid
{
    [Column("Name"), Required, MaxLength(1024)]
    public required string Name { get; set; }

    [Column("Data")]
    public JsonDocument? Data { get; set; }

    public virtual View? View { get; set; }
}