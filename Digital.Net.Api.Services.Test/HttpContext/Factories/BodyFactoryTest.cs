using System.Net.Http.Json;
using Digital.Net.Api.Services.HttpContext.Factories;
using Digital.Net.Api.Services.HttpContext.Factories.models;
using Digital.Net.Tests.Core;

namespace Digital.Net.Api.Services.Test.HttpContext.Factories;

public class BodyFactoryTest : UnitTest
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
            await Assert.That(actualItem.Op).IsEqualTo(expectedItem.Op);
            await Assert.That(actualItem.Path).IsEqualTo(expectedItem.Path);
            await Assert.That(actualItem.Value).IsEqualTo(expectedItem.Value);
        }
    }

    [Test]
    public async Task BuildPatchTest()
    {
        var payload = new { Name = "John", Age = 30 };
        var actual = BodyFactory.BuildPatch(payload);
        await Assert.That(actual).IsTypeOf<JsonContent>();
    }
}