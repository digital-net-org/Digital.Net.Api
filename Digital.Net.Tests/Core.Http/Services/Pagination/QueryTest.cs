using Digital.Net.Core.Http.Services.Pagination;

namespace Digital.Net.Tests.Core.Http.Services.Pagination;

public class QueryTest : UnitTest
{
    [Test]
    public async Task ValidateParameters_SetsDefaultIndex_WhenIndexIsLessThanOne()
    {
        var query = new Query { Index = -1, Size = 1 };
        query.ValidateParameters();
        await Assert.That(query.Index).IsEqualTo(Query.DefaultIndex);
    }

    [Test]
    public async Task ValidateParameters_SetsDefaultSize_WhenSizeIsLessThanOne()
    {
        var query = new Query { Index = 1, Size = -1 };
        query.ValidateParameters();
        await Assert.That(query.Size).IsEqualTo(Query.DefaultSize);
    }

    [Test]
    public async Task ValidateParameters_KeepsSize_WhenWithinRange()
    {
        var query = new Query { Index = 1, Size = 75 };
        query.ValidateParameters();
        await Assert.That(query.Size).IsEqualTo(75);
    }

    [Test]
    public async Task ValidateParameters_CapsSize_WhenAboveMax()
    {
        var query = new Query { Index = 1, Size = 1_000_000 };
        query.ValidateParameters();
        await Assert.That(query.Size).IsEqualTo(Query.MaxSize);
    }
}