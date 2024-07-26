using System.Linq.Expressions;
using SafariDigital.Core.Predicate;
using Tests.Core.Base;

namespace Tests.Unit.SafariDigital.Core.Predicate;

public class PredicateBuilderTest : UnitTest
{
    [Fact]
    public void New_ReturnsCorrectExpression()
    {
        var expression = PredicateBuilder.New<int>(true);
        var compiledExpression = expression.Compile();
        Assert.True(compiledExpression(0));
    }

    [Fact]
    public void And_ReturnsCombinedExpression()
    {
        Expression<Func<int, bool>> expr1 = x => x > 5;
        Expression<Func<int, bool>> expr2 = x => x < 10;
        var combinedExpression = expr1.And(expr2);
        var compiledExpression = combinedExpression.Compile();
        Assert.True(compiledExpression(7));
        Assert.False(compiledExpression(4));
        Assert.False(compiledExpression(11));
    }
}