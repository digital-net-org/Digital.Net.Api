using Digital.Net.Api.Controllers.Generic.Pagination;
using Digital.Net.Tests.Core;

namespace Digital.Net.Api.Controllers.Test.Generic.Pagination;

public class PaginationUtilsTest : UnitTest
{
    [Test]
    public async Task ValidateParameters_SetsDefaultIndex_WhenIndexIsLessThanOne()
    {
        var query = new Query { Index = -1, Size = 1 };
        query.ValidateParameters();
        await Assert.That(query.Index).IsEqualTo(PaginationUtils.DefaultIndex);
    }

    [Test]
    public async Task ValidateParameters_SetsDefaultSize_WhenSizeIsLessThanOne()
    {
        var query = new Query { Index = 1, Size = -1 };
        query.ValidateParameters();
        await Assert.That(query.Size).IsEqualTo(PaginationUtils.DefaultSize);
    }
}