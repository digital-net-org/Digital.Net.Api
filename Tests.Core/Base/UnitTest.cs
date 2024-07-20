namespace Tests.Core.Base;

public abstract class UnitTest
{
    static UnitTest()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
    }
}