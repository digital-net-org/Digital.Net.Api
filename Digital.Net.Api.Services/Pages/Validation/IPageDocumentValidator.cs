using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Models;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Services.Pages.Validation;

public interface IPageDocumentValidator<T> where T : Entity
{
    public Result ValidateUpload(IFormFile file);
    public Result<T> ValidateDelete(Guid id);
}