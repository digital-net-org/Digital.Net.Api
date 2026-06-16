using System.ComponentModel.DataAnnotations;
using Digital.Net.Cms.Models.Medias;
using Digital.Net.Lib.Entities.Exceptions;
using Digital.Net.Lib.Entities.Pivots;

namespace Digital.Net.Cms.Http.Dto;

/// <summary>
///     Shared base for pivot payloads exposing a media gallery.
/// </summary>
public abstract class MediaPivotPayloadDto<TSelf, TPivot> : IPivotPayload<TSelf, TPivot, Media>
    where TSelf : MediaPivotPayloadDto<TSelf, TPivot>
    where TPivot : class
{
    public Guid? Id { get; set; }

    [Required]
    public required string Label { get; set; }

    public Media ToChild() =>
        throw new EntityValidationException(
            "/media: Media creation require a file upload; upload it first using the \"cms/media\" API."
        );

    public void ApplyTo(Media child) =>
        throw new EntityValidationException(
            "/media: Media cannot be mutated from here; use the \"cms/media\" API."
        );

    public abstract void ApplyToPivot(TPivot pivot);
}