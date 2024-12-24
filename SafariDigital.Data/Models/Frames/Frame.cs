using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Digital.Net.Entities.Models;
using Microsoft.EntityFrameworkCore;
using SafariDigital.Data.Models.Views;

namespace SafariDigital.Data.Models.Frames;

[Table("Frame"), Index(nameof(Name), IsUnique = true)]
public class Frame : EntityGuid
{
    private string? _data;

    [Column("Name"), Required, MaxLength(1024)]
    public required string Name { get; set; }

    [Column("Data")]
    public string? Data
    {
        get => _data;
        set => _data = value is not null ? Convert.ToBase64String(Encoding.UTF8.GetBytes(value)) : null;
    }

    public virtual View? View { get; set; }
}