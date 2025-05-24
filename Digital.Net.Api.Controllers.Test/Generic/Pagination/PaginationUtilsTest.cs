using Digital.Net.Api.Controllers.Generic.Pagination;
using Digital.Net.Api.TestUtilities;
using Xunit;

namespace Digital.Net.Api.Controllers.Test.Generic.Pagination;

public class PaginationUtilsTest : UnitTest
{
    [Fact]
    public void ValidateParameters_SetsDefaultIndex_WhenIndexIsLessThanOne()
    {
        var query = new Query { Index = -1, Size = 1 };
        query.ValidateParameters();
        Assert.Equal(PaginationUtils.DefaultIndex, query.Index);
    }

    [Fact]
    public void ValidateParameters_SetsDefaultSize_WhenSizeIsLessThanOne()
    {
        var query = new Query { Index = 1, Size = -1 };
        query.ValidateParameters();
        Assert.Equal(PaginationUtils.DefaultSize, query.Size);
    }
}