using SafariDigital.Core.Enum;

namespace SafariDigital.Core.Validation;

public class ResultMessage
{
    public ResultMessage(Exception ex)
    {
        Message = ex.Message;
        StackTrace = ex.StackTrace;
    }

    public ResultMessage(System.Enum message)
    {
        Code = message.GetHashCode();
        Reference = message.ToString();
        Message = message.GetDisplayName();
    }

    public ResultMessage(Exception ex, System.Enum message)
    {
        Code = message.GetHashCode();
        Reference = message.ToString();
        Message = message.GetDisplayName();
        StackTrace = ex.StackTrace;
    }

    public int? Code { get; init; }
    public string? Reference { get; init; }
    public string? Message { get; init; }
    public string? StackTrace { get; init; }
}