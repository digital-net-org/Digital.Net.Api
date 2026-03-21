namespace Digital.Net.Cms.Endpoints.Dto;

public class FormSubmitPayload
{
    public Dictionary<string, string?> Values { get; set; } = [];

    public string? SubmitterIp { get; set; }

    public string? UserAgent { get; set; }
}
