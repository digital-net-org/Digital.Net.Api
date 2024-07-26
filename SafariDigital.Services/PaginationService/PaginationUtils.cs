using SafariDigital.Services.PaginationService.Models;

namespace SafariDigital.Services.PaginationService;

public static class PaginationUtils
{
    public const int DefaultIndex = 1;
    public const int DefaultSize = 50;
    public const int MaxSize = 200;

    public static void ValidateParameters(this PaginationQuery query)
    {
        query.Index = query.Index < 1 ? DefaultIndex : query.Index;
        query.Size = query.Size is < 1 or > MaxSize ? DefaultSize : query.Size;
    }
}