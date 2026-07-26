using System.Security.Cryptography;
using Digital.Net.Lib.Random;
using Digital.Net.Tests.Core;

namespace Digital.Net.Tests.Lib.Random;

public class RandomizerTest : UnitTest
{
    [Test]
    public async Task GenerateRandomString_RespectsLengthAndCharset()
    {
        var value = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 128);
        await Assert.That(value.Length).IsEqualTo(128);
        await Assert.That(value.All(Randomizer.AnyLetterOrNumber.Contains)).IsTrue();
    }

    [Test]
    public async Task GenerateRandomString_DoesNotRepeat()
    {
        var values = Enumerable
            .Range(0, 50)
            .Select(_ => Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 128))
            .ToList();
        await Assert.That(values.Distinct().Count()).IsEqualTo(values.Count);
    }

    [Test]
    public async Task GenerateRandomInt_StaysWithinRange()
    {
        for (var i = 0; i < 100; i++)
        {
            var value = RandomNumberGenerator.GetInt32(5, 10);
            await Assert.That(value).IsGreaterThanOrEqualTo(5);
            await Assert.That(value).IsLessThan(10);
        }
    }
}