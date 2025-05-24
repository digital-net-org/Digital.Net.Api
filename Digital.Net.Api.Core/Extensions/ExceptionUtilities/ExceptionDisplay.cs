using Digital.Net.Api.Core.Extensions.StringUtilities;

namespace Digital.Net.Api.Core.Extensions.ExceptionUtilities;

public static class ExceptionDisplay
{

    /// <summary>
    ///     Get the reference for the exception.
    ///     This is the object name of the exception displayed in uppercase snake case.
    /// </summary>
    /// <param name="ex">The exception to get the reference for.</param>
    /// <returns>The reference for the exception.</returns>
    public static string GetReference(this Exception ex)
    {
        var objectName = ex.GetType().ToString();
        return RegularExpressions.ObjectName().Replace(objectName, "_").ToUpper();
    }

    /// <summary>
    ///     Get the formatted error code for the exception.
    ///     This is the HResult of the exception formatted as a hexadecimal string.
    /// </summary>
    /// <param name="ex">The exception to get the formatted error code for.</param>
    /// <returns>The formatted error code for the exception.</returns>
    public static string GetFormattedErrorCode(this Exception ex) => $"0x{ex.HResult:X8}";
}