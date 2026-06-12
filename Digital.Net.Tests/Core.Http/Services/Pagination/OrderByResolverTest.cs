using Digital.Net.Core.Http.Services.Pagination;

namespace Digital.Net.Tests.Core.Http.Services.Pagination;

public class OrderByResolverTest : UnitTest
{
    private class Sample
    {
        public DateTime CreatedAt { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    [Test]
    public async Task Resolve_ReturnsDefault_WhenNullOrBlank()
    {
        await Assert.That(OrderByResolver.Resolve<Sample>(null)).IsEqualTo(OrderByResolver.DefaultColumn);
        await Assert.That(OrderByResolver.Resolve<Sample>("  ")).IsEqualTo(OrderByResolver.DefaultColumn);
    }

    [Test]
    public async Task Resolve_ReturnsCanonicalName_WhenKnownColumn()
    {
        // Case-insensitive match resolves to the property's canonical casing.
        await Assert.That(OrderByResolver.Resolve<Sample>("title")).IsEqualTo("Title");
    }

    [Test]
    public async Task Resolve_FallsBackToDefault_WhenUnknownColumn()
    {
        await Assert.That(OrderByResolver.Resolve<Sample>("DROP TABLE")).IsEqualTo(OrderByResolver.DefaultColumn);
        await Assert.That(OrderByResolver.Resolve<Sample>("NotAProperty")).IsEqualTo(OrderByResolver.DefaultColumn);
    }
}
