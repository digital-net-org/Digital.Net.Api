using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Digital.Net.Entities.Models;
using Microsoft.EntityFrameworkCore;
using SafariDigital.Data.Models.Database.Views;

namespace SafariDigital.Data.Models.Database.Frames;

[Table("frame"), Index(nameof(Name), IsUnique = true)]
public class Frame : EntityWithGuid
{
    [Column("name"), Required, MaxLength(1024)]
    public required string Name { get; set; }

    [Column("data")]
    public string? Data
    {
        get => _data;
        set => _data = value is not null ? Convert.ToBase64String(Encoding.UTF8.GetBytes(value)) : null;
    }

    private string? _data;

    public virtual View? View { get; set; }
}
