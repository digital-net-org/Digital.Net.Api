using Digital.Net.Api.Core.Random;
using Digital.Net.Api.TestUtilities;

namespace Digital.Net.Api.Core.Test.Random;

public class RandomizerTests : UnitTest
{
    [Fact]
    public void GenerateRandomString_ReturnsStringOfCorrectLength_WhenLengthIsSet()
    {
        const string chars = "ABC";
        const int length = 10;
        var result = Randomizer.GenerateRandomString(chars, length);
        Assert.Equal(length, result.Length);
    }

    [Fact]
    public void GenerateRandomString_ReturnsStringWithOnlySpecifiedChars_WhenCharsIsSet()
    {
        const string chars = "ABC";
        var result = Randomizer.GenerateRandomString(chars);
        Assert.All(result, c => Assert.Contains(c, chars));
    }

    [Fact]
    public void GenerateRandomString_ReturnsDifferentStringsOnSubsequentCalls()
    {
        var result1 = Randomizer.GenerateRandomString();
        var result2 = Randomizer.GenerateRandomString();
        var result3 = Randomizer.GenerateRandomString("ABC", 10);
        var result4 = Randomizer.GenerateRandomString("ABC", 10);
        var result5 = Randomizer.GenerateRandomString("ABC");
        var result6 = Randomizer.GenerateRandomString("ABC");
        Assert.NotEqual(result1, result2);
        Assert.NotEqual(result3, result4);
        Assert.NotEqual(result5, result6);
    }

    [Fact]
    public void GenerateRandomInt_ReturnsIntegerWithinSpecifiedRange_WhenRangeIsSet()
    {
        const int rangeMin = 1;
        const int rangeMax = 100;
        var result1 = Randomizer.GenerateRandomInt(rangeMin, rangeMax);
        var result2 = Randomizer.GenerateRandomInt(rangeMax);
        Assert.InRange(result1, rangeMin, rangeMax);
        Assert.InRange(result2, -rangeMax, rangeMax);
    }

    [Fact]
    public void GenerateRandomInt_ReturnsDifferentIntegersOnSubsequentCalls()
    {
        var result1 = Randomizer.GenerateRandomInt();
        var result2 = Randomizer.GenerateRandomInt();
        var result3 = Randomizer.GenerateRandomInt(1, 100);
        var result4 = Randomizer.GenerateRandomInt(1, 100);
        var result5 = Randomizer.GenerateRandomInt(100);
        var result6 = Randomizer.GenerateRandomInt(100);
        Assert.NotEqual(result1, result2);
        Assert.NotEqual(result3, result4);
        Assert.NotEqual(result5, result6);
    }

    [Fact]
    public void GenerateRandomEmail_ReturnsEmailWithCorrectValues_WhenParamsAreSets()
    {
        const string domain = "test";
        const string topLevelDomain = "com";
        var result = Randomizer.GenerateRandomEmail(domain, topLevelDomain);
        Assert.Contains(domain, result);
        Assert.Contains(topLevelDomain, result);
    }
}