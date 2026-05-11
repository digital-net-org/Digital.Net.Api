using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Cms.Endpoints.Dto;

public class ArticlePayload
{
    [Required]
    public required string Slug { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Content { get; set; }

    public Guid? PageId { get; set; }
}
