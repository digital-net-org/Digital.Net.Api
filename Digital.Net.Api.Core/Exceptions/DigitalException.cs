using Digital.Net.Api.Core.Extensions.StringUtilities;

namespace Digital.Net.Api.Core.Exceptions;

public abstract class DigitalException : Exception
{
    protected DigitalException()
    {
        OnInstantiation();
    }

    protected DigitalException(string message) : base(message)
    {
        OnInstantiation();
    }

    private void OnInstantiation() => Code = GetType().Name.ToUpperSnakeCase();

    public string Code { get; private set; }
}