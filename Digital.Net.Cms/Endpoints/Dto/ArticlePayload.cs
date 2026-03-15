using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Cms.Endpoints.Dto;

public class ArticlePayload
{
    [Required]
    public required string Path { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Content { get; set; }
}
