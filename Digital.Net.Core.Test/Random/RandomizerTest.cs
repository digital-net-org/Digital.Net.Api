using Digital.Net.Core.Random;
using Digital.Net.Tests.Core;

namespace Digital.Net.Core.Test.Random;

public class RandomizerTests : UnitTest
{
    [Test]
    public async Task GenerateRandomString_ReturnsStringOfCorrectLength_WhenLengthIsSet()
    {
        const string chars = "ABC";
        const int length = 10;
        var result = Randomizer.GenerateRandomString(chars, length);
        await Assert.That(result.Length).IsEqualTo(length);
    }

    [Test]
    public async Task GenerateRandomString_ReturnsStringWithOnlySpecifiedChars_WhenCharsIsSet()
    {
        const string chars = "ABC";
        var result = Randomizer.GenerateRandomString(chars);
        foreach (var c in result)
            await Assert.That(chars.Contains(c)).IsTrue();
    }

    [Test]
    public async Task GenerateRandomString_ReturnsDifferentStringsOnSubsequentCalls()
    {
        var result1 = Randomizer.GenerateRandomString();
        var result2 = Randomizer.GenerateRandomString();
        var result3 = Randomizer.GenerateRandomString("ABC", 10);
        var result4 = Randomizer.GenerateRandomString("ABC", 10);
        var result5 = Randomizer.GenerateRandomString("ABC");
        var result6 = Randomizer.GenerateRandomString("ABC");
        await Assert.That(result1).IsNotEqualTo(result2);
        await Assert.That(result3).IsNotEqualTo(result4);
        await Assert.That(result5).IsNotEqualTo(result6);
    }

    [Test]
    public async Task GenerateRandomInt_ReturnsIntegerWithinSpecifiedRange_WhenRangeIsSet()
    {
        const int rangeMin = 1;
        const int rangeMax = 100;
        var result1 = Randomizer.GenerateRandomInt(rangeMin, rangeMax);
        var result2 = Randomizer.GenerateRandomInt(rangeMax);
        await Assert.That(result1).IsBetween(rangeMin, rangeMax);
        await Assert.That(result2).IsBetween(-rangeMax, rangeMax);
    }

    [Test]
    public async Task GenerateRandomInt_ReturnsDifferentIntegersOnSubsequentCalls()
    {
        var result1 = Randomizer.GenerateRandomInt();
        var result2 = Randomizer.GenerateRandomInt();
        var result3 = Randomizer.GenerateRandomInt(1, 100);
        var result4 = Randomizer.GenerateRandomInt(1, 100);
        var result5 = Randomizer.GenerateRandomInt(100);
        var result6 = Randomizer.GenerateRandomInt(100);
        await Assert.That(result1).IsNotEqualTo(result2);
        await Assert.That(result3).IsNotEqualTo(result4);
        await Assert.That(result5).IsNotEqualTo(result6);
    }

    [Test]
    public async Task GenerateRandomEmail_ReturnsEmailWithCorrectValues_WhenParamsAreSets()
    {
        const string domain = "test";
        const string topLevelDomain = "com";
        var result = Randomizer.GenerateRandomEmail(domain, topLevelDomain);
        await Assert.That(result.Contains(domain)).IsTrue();
        await Assert.That(result.Contains(topLevelDomain)).IsTrue();
    }
}