using Digital.Net.Lib.Environment;

namespace Digital.Net.Tests.Core;

public abstract class UnitTest
{
    protected UnitTest()
    {
        AspNetEnv.Set(AspNetEnv.Test);
    }
}