namespace Digital.Net.Api.Core.Errors;

public static class TryCatchUtilities
{
    /// <summary>
    ///     Try to execute a function and return the result or null if an exception is thrown.
    ///     Use this method when you want to ignore exceptions and return null instead.
    /// </summary>
    /// <param name="func">The function to execute.</param>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <returns>The result of the function if it executes successfully, otherwise null.</returns>
    public static T? TryOrNull<T>(Func<T> func) where T : class
    {
        try
        {
            return func();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Try to execute a function and return the result or null if an exception is thrown.
    ///     Use this method when you want to ignore exceptions and return null instead.
    /// </summary>
    /// <param name="func">The function to execute.</param>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <returns>The result of the function if it executes successfully, otherwise null.</returns>
    public static T? TryOrNull<T>(Func<T?> func) where T : struct
    {
        try
        {
            return func();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Try to execute a function and return the result or null if an exception is thrown.
    ///     Use this method when you want to ignore exceptions and return null instead.
    /// </summary>
    /// <param name="func">The function to execute.</param>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <returns>The result of the function if it executes successfully, otherwise null.</returns>
    public static async Task<T?> TryOrNullAsync<T>(Func<Task<T>> func) where T : class
    {
        try
        {
            return await func();
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    ///     Try to execute a function and return the result or null if an exception is thrown.
    ///     Use this method when you want to ignore exceptions and return null instead.
    /// </summary>
    /// <param name="func">The function to execute.</param>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <returns>The result of the function if it executes successfully, otherwise null.</returns>
    public static async Task<T?> TryOrNullAsync<T>(Func<Task<T?>> func) where T : struct
    {
        try
        {
            return await func();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Try all cases until returns a value.
    /// </summary>
    /// <param name="cases">The cases to try.</param>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <returns>The result of the first case that returns a value.</returns>
    public static T? TryAll<T>(params Func<T?>[] cases) where T : struct
    {
        foreach (var @case in cases)
        {
            var result = TryOrNull(@case);
            if (result is not null)
                return result;
        }

        return default;
    }

    /// <summary>
    ///     Try all cases until returns a value.
    /// </summary>
    /// <param name="cases">The cases to try.</param>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <returns>The result of the first case that returns a value.</returns>
    public static T? TryAll<T>(params Func<T>[] cases) where T : class =>
        cases.Select(TryOrNull).OfType<T>().FirstOrDefault();
}