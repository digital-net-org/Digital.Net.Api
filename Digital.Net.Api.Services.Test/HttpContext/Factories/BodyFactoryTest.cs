using Digital.Net.Api.Services.HttpContext.Factories;
using Digital.Net.Api.Services.HttpContext.Factories.models;
using Xunit;

namespace Digital.Net.Api.Services.Test.HttpContext.Factories;

public class BodyFactoryTest
{
    [Fact]
    public void BuildPatchRowsTest()
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
            Assert.Equal(expectedItem.Op, actualItem.Op);
            Assert.Equal(expectedItem.Path, actualItem.Path);
            Assert.Equal(expectedItem.Value, actualItem.Value);
        }
    }
}