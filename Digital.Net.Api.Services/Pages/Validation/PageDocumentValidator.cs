using Digital.Net.Api.Core.Exceptions;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models;
using Digital.Net.Api.Entities.Repositories;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Services.Pages.Validation;

public abstract class PageDocumentValidator<T>(
    IRepository<T, DigitalContext> repository
) : IPageDocumentValidator<T> where T : Entity
{
    public abstract List<string> GetAllowedMimeTypes();

    public virtual Result ValidateUpload(IFormFile file)
    {
        var result = new Result();
        if (file.Length == 0)
            return result.AddError(new ResourceMalformedException());
        if (!GetAllowedMimeTypes().Contains(file.ContentType))
            return result.AddError(new ResourceContentTypeException());
        if (string.IsNullOrEmpty(file.FileName))
            return result.AddError(new ResourceMalformedException());
        return result;
    }

    public virtual Result<T> ValidateDelete(int id)
    {
        var result = new Result<T>();
        var config = repository.GetById(id);
        if (config is null)
            return result.AddError(new ResourceNotFoundException());
        result.Value = config;
        return result;
    }
}