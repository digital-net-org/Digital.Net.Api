using Digital.Net.Cms.Models;
using Digital.Net.Cms.Models.Forms;

namespace Digital.Net.Cms.Endpoints.Dto;

public class FormSubmissionDto
{
    public FormSubmissionDto()
    {
    }

    public FormSubmissionDto(FormSubmission submission)
    {
        Id = submission.Id;
        FormId = submission.FormId;
        ValuesJson = submission.ValuesJson;
        SubmitterIp = submission.SubmitterIp;
        UserAgent = submission.UserAgent;
        CreatedAt = submission.CreatedAt;
        UpdatedAt = submission.UpdatedAt;
    }

    public Guid Id { get; init; }
    public Guid FormId { get; init; }
    public string ValuesJson { get; set; } = string.Empty;
    public string? SubmitterIp { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
