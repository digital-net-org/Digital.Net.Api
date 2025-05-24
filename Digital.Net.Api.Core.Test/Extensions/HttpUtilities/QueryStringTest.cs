using Digital.Net.Api.Core.Extensions.HttpUtilities;
using Digital.Net.Api.TestUtilities;

namespace Digital.Net.Api.Core.Test.Extensions.HttpUtilities;

public class QueryStringTests : UnitTest
{
    [Fact]
    public void ToQueryString_ReturnsQueryString_WhenQueryObjectIsNotNull()
    {
        var query = new { TestProperty = "TestValue" };
        var result = query.ToQueryString();
        Assert.Equal("?TestProperty=TestValue", result);
    }
}
