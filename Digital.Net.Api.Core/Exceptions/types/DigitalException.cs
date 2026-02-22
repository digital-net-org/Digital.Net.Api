using Digital.Net.Api.Core.String;

namespace Digital.Net.Api.Core.Exceptions.types;

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