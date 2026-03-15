using Digital.Net.Lib.String;

namespace Digital.Net.Lib.Exceptions.types;

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