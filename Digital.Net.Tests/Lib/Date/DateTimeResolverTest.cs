using Digital.Net.Lib.Date;

namespace Digital.Net.Tests.Lib.Date;

public class DateTimeResolverTest
{
    [Test]
    public async Task MaxUpdatedAt_ReturnsNull_WhenBothAreNull()
    {
        var result = DateTimeResolver.MaxUpdatedAt(null, null);
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task MaxUpdatedAt_ReturnsB_WhenAIsNull()
    {
        var b = new DateTime(2026, 5, 4, 10, 0, 0, DateTimeKind.Utc);
        var result = DateTimeResolver.MaxUpdatedAt(null, b);
        await Assert.That(result).IsEqualTo(b);
    }

    [Test]
    public async Task MaxUpdatedAt_ReturnsA_WhenBIsNull()
    {
        var a = new DateTime(2026, 5, 4, 10, 0, 0, DateTimeKind.Utc);
        var result = DateTimeResolver.MaxUpdatedAt(a, null);
        await Assert.That(result).IsEqualTo(a);
    }

    [Test]
    public async Task MaxUpdatedAt_ReturnsA_WhenAIsGreater()
    {
        var a = new DateTime(2026, 5, 4, 12, 0, 0, DateTimeKind.Utc);
        var b = new DateTime(2026, 5, 4, 10, 0, 0, DateTimeKind.Utc);
        var result = DateTimeResolver.MaxUpdatedAt(a, b);
        await Assert.That(result).IsEqualTo(a);
    }

    [Test]
    public async Task MaxUpdatedAt_ReturnsB_WhenBIsGreater()
    {
        var a = new DateTime(2026, 5, 4, 10, 0, 0, DateTimeKind.Utc);
        var b = new DateTime(2026, 5, 4, 12, 0, 0, DateTimeKind.Utc);
        var result = DateTimeResolver.MaxUpdatedAt(a, b);
        await Assert.That(result).IsEqualTo(b);
    }

    [Test]
    public async Task MaxUpdatedAt_ReturnsValue_WhenBothAreEqual()
    {
        var date = new DateTime(2026, 5, 4, 10, 0, 0, DateTimeKind.Utc);
        var result = DateTimeResolver.MaxUpdatedAt(date, date);
        await Assert.That(result).IsEqualTo(date);
    }

    [Test]
    public async Task MaxUpdatedAt_HandlesMinAndMaxValue()
    {
        var result = DateTimeResolver.MaxUpdatedAt(DateTime.MinValue, DateTime.MaxValue);
        await Assert.That(result).IsEqualTo(DateTime.MaxValue);
    }
}
