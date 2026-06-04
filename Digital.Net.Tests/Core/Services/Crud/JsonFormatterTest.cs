using System.Text.Json;
using Digital.Net.Core.Http.Services.Crud;

namespace Digital.Net.Tests.Core.Services.Crud;

public class JsonFormatterTest : UnitTest
{
    private class TestModel
    {
        public string? Name { get; set; }
    }

    [Test]
    public async Task GetPatchDocument_ReturnsCorrectPatchDocument()
    {
        using var jsonDoc = JsonDocument.Parse(@"[
            {
                ""op"": ""replace"",
                ""path"": ""/name"",
                ""value"": ""NewName""
            }
        ]");
        var patchDocument = jsonDoc.RootElement.GetPatchDocument<TestModel>();
        await Assert.That(patchDocument).IsNotNull();
        await Assert.That(patchDocument.Operations.Count).IsEqualTo(1);

        var operation = patchDocument.Operations.First();
        await Assert.That(operation.op).IsEqualTo("replace");
        await Assert.That(operation.path).IsEqualTo("/name");
        await Assert.That(operation.value).IsEqualTo("NewName");
    }

    [Test]
    public async Task GetPatchDocument_ReturnsEmptyDocument_WhenJsonIsEmpty()
    {
        var patchJson = @"[]";
        using var jsonDoc = JsonDocument.Parse(patchJson);
        var patchDocument = jsonDoc.RootElement.GetPatchDocument<TestModel>();

        await Assert.That(patchDocument).IsNotNull();
        await Assert.That(patchDocument.Operations.Count).IsEqualTo(0);
    }
}
