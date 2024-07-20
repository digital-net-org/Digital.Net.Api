using SafariDigital.Core.Random;
using Tests.Core.Base;

namespace Tests.Unit.SafariDigital.Core.Random;

public class RandomUtilsTest : UnitTest
{
    [Fact]
    public void GenerateRandomSecret_Should_return_a_random_string()
    {
        // Act
        var secret = RandomUtils.GenerateRandomSecret();

        // Assert
        Assert.NotNull(secret);
        Assert.NotEmpty(secret);
        Assert.Equal(128, secret.Length);
    }
}