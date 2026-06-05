using Digital.Net.Core.Http.Services.Pagination;

namespace Digital.Net.Cms.Http.Dto;

public class FormSubmissionQuery : Query
{
    public Guid? FormId { get; set; }
}