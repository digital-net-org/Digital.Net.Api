namespace Digital.Net.Api.Controllers.Generic.Pagination;

public static class PaginationUtils
{
    public const int DefaultIndex = 1;
    public const int DefaultSize = 50;

    public static void ValidateParameters(this Query query)
    {
        query.Index = query.Index < 1 ? DefaultIndex : query.Index;
        query.Size = query.Size < 1 ? DefaultSize : query.Size;
    }
}