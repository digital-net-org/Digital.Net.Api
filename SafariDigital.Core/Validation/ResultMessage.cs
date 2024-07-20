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
        Code = message;
        Message = message.GetDisplayName();
    }

    public ResultMessage(Exception ex, System.Enum message)
    {
        Code = message;
        Message = message.GetDisplayName();
        StackTrace = ex.StackTrace;
    }

    public System.Enum? Code { get; init; }
    public string? Reference => Code?.ToString();
    public string? Message { get; init; }
    public string? StackTrace { get; init; }
}