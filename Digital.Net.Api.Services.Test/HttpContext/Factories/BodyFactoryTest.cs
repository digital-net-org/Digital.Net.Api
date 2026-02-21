using Digital.Net.Api.Services.HttpContext.Factories;
using Digital.Net.Api.Services.HttpContext.Factories.models;

namespace Digital.Net.Api.Services.Test.HttpContext.Factories;

public class BodyFactoryTest
{
    [Test]
    public async Task BuildPatchRowsTest()
    {
        var payload = new { Name = "John", Age = 30 };
        var expected =
            new List<PatchRow>
            {
                new("replace", "/Name", "John"),
                new("replace", "/Age", 30)
            };
        var actual = BodyFactory.BuildPatchRows(payload);
        foreach (var (actualItem, expectedItem) in actual.Zip(expected))
        {
            await Assert.That(actualItem.Op).EqualTo(expectedItem.Op);
            await Assert.That(actualItem.Path).EqualTo(expectedItem.Path);
            await Assert.That(actualItem.Value).EqualTo(expectedItem.Value);
        }
    }
}