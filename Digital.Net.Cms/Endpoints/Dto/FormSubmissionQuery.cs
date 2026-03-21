using Digital.Net.Core.Services.Pagination;

namespace Digital.Net.Cms.Endpoints.Dto;

public class FormSubmissionQuery : Query
{
    public Guid? FormId { get; set; }
}
