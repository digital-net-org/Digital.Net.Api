using System.ComponentModel.DataAnnotations;

namespace SafariDigital.Database.Models.DocumentTable;

public enum EMimeType
{
    [Display(Name = "image/png")] Png = 0,
    [Display(Name = "image/jpeg")] Jpg = 1,
    [Display(Name = "image/svg+xml")] Svg = 2,
    [Display(Name = "image/bmp")] Bmp = 3
}