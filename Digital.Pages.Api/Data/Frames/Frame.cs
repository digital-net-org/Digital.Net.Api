using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Lib.Net.Entities.Attributes;
using Digital.Lib.Net.Entities.Models;
using Digital.Pages.Api.Data.Views;
using Microsoft.EntityFrameworkCore;

namespace Digital.Pages.Api.Data.Frames;

[Table("Frame"), Index(nameof(Name), IsUnique = true)]
public class Frame : EntityGuid
{
    [Column("Name"), Required, MaxLength(1024)]
    public required string Name { get; set; }

    [Column("Data"), DataFlag("json")]
    public string? Data { get; set; }

    public virtual View? View { get; set; }
}