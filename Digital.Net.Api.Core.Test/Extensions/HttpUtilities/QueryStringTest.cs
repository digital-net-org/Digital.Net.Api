using Digital.Net.Api.Core.Extensions.HttpUtilities;
using Digital.Net.Tests.Core;

namespace Digital.Net.Api.Core.Test.Extensions.HttpUtilities;

public class QueryStringTests : UnitTest
{
    [Test]
    public async Task ToQueryString_ReturnsQueryString_WhenQueryObjectIsNotNull()
    {
        var query = new { TestProperty = "TestValue" };
        var result = query.ToQueryString();
        await Assert.That(result).IsEqualTo("?TestProperty=TestValue");
    }
}
