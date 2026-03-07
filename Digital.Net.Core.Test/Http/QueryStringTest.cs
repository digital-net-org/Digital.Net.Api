using Digital.Net.Core.Http;
using Digital.Net.Tests.Core;

namespace Digital.Net.Core.Test.Http;

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
