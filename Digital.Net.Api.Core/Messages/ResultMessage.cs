using Digital.Net.Api.Core.Extensions.ExceptionUtilities;

namespace Digital.Net.Api.Core.Messages;

/// <summary>
///     A class to hold the result of a message. Can be created using either an exception or an enum.
/// </summary>
public class ResultMessage
{
    public ResultMessage() {}

    public ResultMessage(Exception ex, string? message = null)
    {
        Code = message?.GetHashCode().ToString() ?? ex.GetFormattedErrorCode();
        Message = message ?? ex.Message;
        Reference = ex.GetReference();
        StackTrace = ex.StackTrace;
        Exception = ex;
    }

    public ResultMessage(string message)
    {
        Reference = "UNREFERENCED_MESSAGE";
        Message = message;
    }

    public void Throw() => throw Exception ?? new Exception(Message);

    public string? Code { get; init; }
    public string? Reference { get; init; }
    public string? Message { get; init; }
    public string? StackTrace { get; init; }
    private Exception? Exception { get; }
    public bool IsExceptionOfType<TException>() where TException : Exception =>
        Exception is TException;
}