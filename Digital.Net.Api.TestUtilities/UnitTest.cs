
using Digital.Net.Api.Core.Environment;

namespace Digital.Net.Api.TestUtilities;

public abstract class UnitTest
{
    protected UnitTest()
    {
        AspNetEnv.Set(AspNetEnv.Test);
    }
}
