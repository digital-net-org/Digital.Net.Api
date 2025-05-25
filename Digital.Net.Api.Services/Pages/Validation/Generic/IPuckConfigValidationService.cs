using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Models.Pages;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Services.Pages.Validation.Generic;

public interface IPuckConfigValidationService : IPageDocumentValidator<PagePuckConfig>
{
    public Result ValidateUpload(IFormFile file, string version);
}