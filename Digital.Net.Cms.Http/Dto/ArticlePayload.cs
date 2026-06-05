using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Cms.Http.Dto;

public class ArticlePayload
{
    [Required]
    public required string Title { get; set; }

    [Required]
    public required string Description { get; set; }

    [Required]
    public required string Content { get; set; }

    [Required]
    public required string Slug { get; set; }

    public DateTime? PublishedAt { get; set; }

    public Guid? PageId { get; set; }
}
