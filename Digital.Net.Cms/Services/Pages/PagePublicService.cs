using Digital.Net.Cms.Context;
using Digital.Net.Cms.Services.Pages.Dto;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Lib.Messages;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Services.Pages;

public class PagePublicService(
    CmsContext context
)
{
    public async Task<Result<List<PageSheetInfoDto>>> GetPageSheetInfos(Guid id)
    {
        var result = new Result<List<PageSheetInfoDto>>();
        try
        {
            var pageExists = await context.Pages.AnyAsync(p => p.Id == id);
            if (!pageExists)
                throw new ResourceNotFoundException();

            result.Value = await context.PageSheets
                .AsNoTracking()
                .Include(ps => ps.Child)
                .Where(ps => ps.ParentId == id && ps.Child.Published == true)
                .OrderBy(ps => ps.Order)
                .Select(ps => new PageSheetInfoDto
                {
                    Id = ps.ChildId,
                    Name = ps.Child.Name,
                    Type = ps.Child.Type
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }

    public async Task<Result<(string contentType, string content)>> GetPageSheetResource(Guid id, Guid sheetId)
    {
        var result = new Result<(string contentType, string content)>();
        try
        {
            var pageSheet = await context.PageSheets
                .AsNoTracking()
                .Include(ps => ps.Child)
                .FirstOrDefaultAsync(ps => ps.ParentId == id && ps.ChildId == sheetId);

            if (pageSheet is null || !pageSheet.Child.Published)
                throw new ResourceNotFoundException();

            var contentType = pageSheet.Child.Type switch
            {
                "css" => "text/css",
                "js" => "application/javascript",
                "html" => "text/html",
                _ => "text/plain"
            };

            result.Value = (contentType, pageSheet.Child.Content);
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }

    public async Task<Result<PagePublicDto>> GetPageByPath(string path)
    {
        var result = new Result<PagePublicDto>();
        try
        {
            result.Value = await context.Pages
                .AsNoTracking()
                .Where(p => p.Path == path && p.Published)
                .Select(p => new PagePublicDto(p))
                .FirstOrDefaultAsync();

            if (result.Value is null)
                throw new ResourceNotFoundException();
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }
}