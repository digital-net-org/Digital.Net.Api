using SafariDigital.Core.Application;
using SafariDigital.Core.Validation;

namespace SafariDigital.Database.Repository;

public static class RepositoryUtils
{
    public static Result<T?> TryQuery<T>(T queryFunc)
    {
        var result = new Result<T?>();
        try
        {
            result.Value = queryFunc;
        }
        catch (Exception e)
        {
            result.AddError(e);
        }

        return result;
    }

    public static async Task<Result<T?>> TryQueryAsync<T>(Task<T> queryFunc)
    {
        var result = new Result<T?>();
        try
        {
            result.Value = await queryFunc;
        }
        catch (Exception e)
        {
            result.AddError(EApplicationMessage.QueryError, e);
        }

        return result;
    }
}