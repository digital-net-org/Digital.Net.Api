using System.ComponentModel.DataAnnotations;
using Digital.Net.Cms.Models;
using Digital.Net.Cms.Models.Articles;
using Digital.Net.Lib.Entities.Pivots;

namespace Digital.Net.Cms.Http.Dto;

public class TagPayload : IPivotPayload<TagPayload, ArticleTag, Tag>
{
    public TagPayload()
    {
    }

    public TagPayload(ArticleTag pivot)
    {
        Id = pivot.ChildId;
        Name = pivot.Child.Name;
        Color = pivot.Child.Color;
    }

    public Guid? Id { get; set; }

    [Required]
    public required string Name { get; set; }

    public string? Color { get; set; }

    public Tag ToChild() => new() { Name = Name, Color = Color };

    public void ApplyTo(Tag child)
    {
        child.Name = Name;
        child.Color = Color;
    }
}
