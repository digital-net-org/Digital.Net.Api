namespace Digital.Net.Cms.Http.Dto;

public class FormSubmissionDto
{
    public FormSubmissionDto()
    {
    }

    public Guid Id { get; init; }
    public Guid FormId { get; init; }
    public string ValuesJson { get; set; } = string.Empty;
    public string? SubmitterIp { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
