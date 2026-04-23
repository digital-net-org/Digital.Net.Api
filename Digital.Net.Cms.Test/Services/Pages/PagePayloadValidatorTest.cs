using System.Text.Json;
using Digital.Net.Cms.Models;
using Digital.Net.Cms.Services.Pages;
using Digital.Net.Core.Services.Crud.Exceptions;
using Digital.Net.Tests.Core;

namespace Digital.Net.Cms.Test.Services.Pages;

public class PagePayloadValidatorTest : UnitTest
{
    private static JsonElement Patch(params object[] ops) => JsonSerializer.SerializeToElement(ops);

    private static Page BuildPage(string path = "/home", PageEntityType? entityType = null) => 
        new() { Path = path, EntityType = entityType };

    private static object OgReplace(object? value) => new { op = "replace", path = "/openGraph", value };
    private static object Entry(string property, string content) => new { property, content };

    // --- EnsureEntityTypeConsistency ---

    [Test]
    public async Task EnsureEntityTypeConsistency_ReturnsOk_WhenEntityTypeNull()
    {
        var result = PagePayloadValidator.ValidateEntityTypeConsistency("/home", null);
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task EnsureEntityTypeConsistency_ReturnsOk_WhenPathHasDynamicSlug()
    {
        var result = PagePayloadValidator.ValidateEntityTypeConsistency("/articles/:id", PageEntityType.Article);
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task EnsureEntityTypeConsistency_ReturnsOk_WhenPathHasMultipleSlugs()
    {
        var result =
            PagePayloadValidator.ValidateEntityTypeConsistency("/blog/:slug/comments/:cid", PageEntityType.Form);
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    [Arguments(PageEntityType.Article)]
    [Arguments(PageEntityType.Form)]
    public async Task EnsureEntityTypeConsistency_ReturnsError_WhenPathHasNoSlug(PageEntityType entityType)
    {
        var result = PagePayloadValidator.ValidateEntityTypeConsistency("/home", entityType);
        await Assert.That(result.HasError).IsTrue();
        await Assert.That(result.HasErrorOfType<EntityValidationException>()).IsTrue();
    }

    [Test]
    public async Task EnsureEntityTypeConsistency_ReturnsError_WhenPathIsEmpty()
    {
        var result = PagePayloadValidator.ValidateEntityTypeConsistency(string.Empty, PageEntityType.Article);
        await Assert.That(result.HasError).IsTrue();
    }

    [Test]
    public async Task EnsureEntityTypeConsistency_ErrorMessageMentionsEntityTypeAndSlug()
    {
        var result = PagePayloadValidator.ValidateEntityTypeConsistency("/home", PageEntityType.Article);
        var message = result.Errors[0].Message ?? "";
        await Assert.That(message).Contains("EntityType");
        await Assert.That(message).Contains("dynamic slug");
    }

    // --- NormalizePatch ---

    [Test]
    public async Task NormalizePatch_ReturnsSamePatch_WhenNoOpenGraphOp()
    {
        var patch = Patch(new { op = "replace", path = "/Title", value = "x" });
        var result = PagePayloadValidator.NormalizePatch(patch);
        await Assert.That(result.GetRawText()).IsEqualTo(patch.GetRawText());
    }

    [Test]
    public async Task NormalizePatch_TransformsEntriesArrayToJsonString()
    {
        var patch = Patch(OgReplace(new[]
        {
            Entry("og:title", "Hello")
        }));
        var normalized = PagePayloadValidator.NormalizePatch(patch);
        var op = normalized.EnumerateArray().First();
        var value = op.GetProperty("value");
        await Assert.That(value.ValueKind).IsEqualTo(JsonValueKind.String);
        await Assert.That(value.GetString()).Contains("og:title");
        await Assert.That(value.GetString()).Contains("Hello");
    }

    [Test]
    public async Task NormalizePatch_KeepsNullValue()
    {
        var patch = Patch(OgReplace(null));
        var normalized = PagePayloadValidator.NormalizePatch(patch);
        var op = normalized.EnumerateArray().First();
        var value = op.GetProperty("value");
        await Assert.That(value.ValueKind).IsEqualTo(JsonValueKind.Null);
    }

    [Test]
    public async Task NormalizePatch_OnlyTouchesOpenGraphOps()
    {
        var patch = Patch(
            new { op = "replace", path = "/Title", value = "x" },
            OgReplace(new[] { Entry("og:title", "y") })
        );
        var normalized = PagePayloadValidator.NormalizePatch(patch);
        var ops = normalized.EnumerateArray().ToArray();
        await Assert.That(ops[0].GetProperty("value").GetString()).IsEqualTo("x");
        await Assert.That(ops[1].GetProperty("value").ValueKind).IsEqualTo(JsonValueKind.String);
    }

    // --- ValidatePatch ---

    [Test]
    public async Task ValidatePatch_ReturnsOk_WhenPatchEmptyAndCurrentConsistent()
    {
        var result = PagePayloadValidator.ValidatePatch(Patch(), BuildPage());
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task ValidatePatch_ReturnsError_WhenPatchEmptyAndCurrentInconsistent()
    {
        // Inconsistent DB state defensive check: current has EntityType but no slug.
        var result = PagePayloadValidator.ValidatePatch(Patch(), BuildPage("/home", PageEntityType.Article));
        await Assert.That(result.HasError).IsTrue();
    }

    [Test]
    public async Task ValidatePatch_ReturnsError_WhenPathChangeRemovesSlugButEntityTypeRemains()
    {
        var patch = Patch(new { op = "replace", path = "/Path", value = "/home" });
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage("/articles/:id", PageEntityType.Article));
        await Assert.That(result.HasError).IsTrue();
    }

    [Test]
    public async Task ValidatePatch_ReturnsOk_WhenPathAndEntityTypeClearedTogether()
    {
        var patch = Patch(
            new { op = "replace", path = "/Path", value = "/home" },
            new { op = "replace", path = "/EntityType", value = (string?)null }
        );
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage("/articles/:id", PageEntityType.Article));
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task ValidatePatch_ReturnsError_WhenEntityTypeSetOnPathWithoutSlug()
    {
        var patch = Patch(new { op = "replace", path = "/EntityType", value = "Article" });
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage());
        await Assert.That(result.HasError).IsTrue();
    }

    [Test]
    public async Task ValidatePatch_ReturnsOk_WhenEntityTypeSetOnPathWithSlug()
    {
        var patch = Patch(new { op = "replace", path = "/EntityType", value = "Article" });
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage("/articles/:id"));
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task ValidatePatch_AcceptsLowercaseEntityTypeValue()
    {
        var patch = Patch(new { op = "replace", path = "/EntityType", value = "article" });
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage("/articles/:id"));
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task ValidatePatch_MatchesPatchPathCaseInsensitively()
    {
        // '/path' lowercase should still match the 'Path' property (OrdinalIgnoreCase).
        var patch = Patch(new { op = "replace", path = "/path", value = "/home" });
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage("/articles/:id", PageEntityType.Article));
        await Assert.That(result.HasError).IsTrue();
    }

    [Test]
    public async Task ValidatePatch_ReturnsOk_WhenPathChangeKeepsSlug()
    {
        var patch = Patch(new { op = "replace", path = "/Path", value = "/products/:id" });
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage("/articles/:id", PageEntityType.Article));
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task ValidatePatch_UsesCurrentPath_WhenPatchValueForPathIsNull()
    {
        // Defensive: patch with null path should fallback to current.Path for the consistency check.
        var patch = Patch(new { op = "replace", path = "/Path", value = (string?)null });
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage("/articles/:id", PageEntityType.Article));
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task ValidatePatch_IgnoresOperationsOnOtherProperties()
    {
        var patch = Patch(new { op = "replace", path = "/Title", value = "ignored" });
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage("/articles/:id", PageEntityType.Article));
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task ValidatePatch_ReturnsOk_WhenPatchIsNotAnArray()
    {
        var patch = JsonSerializer.SerializeToElement(new { not = "an array" });
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage());
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task ValidatePatch_ReturnsError_WhenPatchIsNotAnArrayAndCurrentInconsistent()
    {
        // When patch can't be parsed, we still enforce the current state.
        var patch = JsonSerializer.SerializeToElement(new { not = "an array" });
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage("/home", PageEntityType.Article));
        await Assert.That(result.HasError).IsTrue();
    }

    [Test]
    public async Task ValidatePatch_ReturnsOk_WhenNoOpenGraphOp()
    {
        var patch = Patch(new { op = "replace", path = "/Title", value = "x" });
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage());
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task ValidatePatch_ReturnsOk_WhenValueIsNull()
    {
        var patch = Patch(OgReplace(null));
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage());
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task ValidatePatch_ReturnsOk_WhenValueIsEmptyArray()
    {
        var patch = Patch(OgReplace(Array.Empty<object>()));
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage());
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task ValidatePatch_ReturnsOk_WhenEntriesAreValid()
    {
        var patch = Patch(OgReplace(new[]
        {
            Entry("og:title", "Hello"),
            Entry("og:description", "World")
        }));
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage());
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task ValidatePatch_ReturnsOk_WhenDuplicateAllowed()
    {
        var patch = Patch(OgReplace(new[]
        {
            Entry("og:image", "https://a"),
            Entry("og:image", "https://b")
        }));
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage());
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task ValidatePatch_ReturnsError_WhenDuplicateNotAllowed()
    {
        var patch = Patch(OgReplace(new[]
        {
            Entry("og:title", "A"),
            Entry("og:title", "B")
        }));
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage());
        await Assert.That(result.HasError).IsTrue();
        await Assert.That(result.HasErrorOfType<EntityValidationException>()).IsTrue();
        var message = result.Errors[0].Message ?? string.Empty;
        await Assert.That(message).Contains("og:title");
        await Assert.That(message).Contains("Duplicate");
    }

    [Test]
    public async Task ValidatePatch_ReturnsError_WhenUnknownKey()
    {
        var patch = Patch(OgReplace(new[]
        {
            Entry("og:unknown", "x")
        }));
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage());
        await Assert.That(result.HasError).IsTrue();
        var message = result.Errors[0].Message ?? string.Empty;
        await Assert.That(message).Contains("og:unknown");
        await Assert.That(message).Contains("Unknown");
    }

    [Test]
    public async Task ValidatePatch_ReturnsError_WhenValueIsNotArray()
    {
        var patch = Patch(OgReplace(new { og_title = "x" }));
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage());
        await Assert.That(result.HasError).IsTrue();
    }

    [Test]
    public async Task ValidatePatch_ReturnsError_WhenEntryMissesProperty()
    {
        var patch = Patch(OgReplace(new[] { new { content = "x" } }));
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage());
        await Assert.That(result.HasError).IsTrue();
    }

    [Test]
    public async Task ValidatePatch_ReturnsError_WhenEntryMissesContent()
    {
        var patch = Patch(OgReplace(new[] { new { property = "og:title" } }));
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage());
        await Assert.That(result.HasError).IsTrue();
    }

    [Test]
    public async Task ValidatePatch_ReturnsError_WhenPropertyIsEmpty()
    {
        var patch = Patch(OgReplace(new[] { Entry("", "x") }));
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage());
        await Assert.That(result.HasError).IsTrue();
    }

    [Test]
    public async Task ValidatePatch_ReturnsError_WhenContentIsEmpty()
    {
        var patch = Patch(OgReplace(new[] { Entry("og:title", "") }));
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage());
        await Assert.That(result.HasError).IsTrue();
        var message = result.Errors[0].Message ?? string.Empty;
        await Assert.That(message).Contains("empty 'content'");
    }

    [Test]
    public async Task ValidatePatch_MatchesOpenGraphPathCaseInsensitively()
    {
        var patch = Patch(new { op = "replace", path = "/OpenGraph", value = (object?)null });
        var result = PagePayloadValidator.ValidatePatch(patch, BuildPage());
        await Assert.That(result.HasError).IsFalse();
    }
}
