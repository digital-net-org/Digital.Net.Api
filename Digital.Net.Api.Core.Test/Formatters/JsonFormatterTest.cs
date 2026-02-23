using System.Text.Json;
using Digital.Net.Api.Core.Formatters;
using Digital.Net.Tests.Core;

namespace Digital.Net.Api.Core.Test.Formatters;

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
        var patchDocument = JsonFormatter.GetPatchDocument<TestModel>(jsonDoc.RootElement);
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
        var patchDocument = JsonFormatter.GetPatchDocument<TestModel>(jsonDoc.RootElement);

        await Assert.That(patchDocument).IsNotNull();
        await Assert.That(patchDocument.Operations.Count).IsEqualTo(0);
    }
}
