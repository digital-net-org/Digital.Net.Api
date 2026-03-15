using Digital.Net.Entities.Crud.Endpoints;
using Digital.Net.Tests.Core;

namespace Digital.Net.Entities.Test.Crud.Controllers;

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
}