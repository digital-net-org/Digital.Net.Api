using System.Text.Json;
using Digital.Net.Cms.Models;
using Digital.Net.Cms.Services.Pages;
using Digital.Net.Core.Services.Crud.Exceptions;
using Digital.Net.Tests.Core;

namespace Digital.Net.Cms.Test.Services.Pages;

public class PageValidatorTest : UnitTest
{
    private static JsonElement Patch(params object[] ops) => JsonSerializer.SerializeToElement(ops);

    private static Page BuildPage(string path = "/home", PageEntityType? entityType = null) =>
        new() { Path = path, EntityType = entityType };

    // --- EnsureEntityTypeConsistency ---

    [Test]
    public async Task EnsureEntityTypeConsistency_ReturnsOk_WhenEntityTypeNull()
    {
        var result = PageValidator.EnsureEntityTypeConsistency("/home", null);
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task EnsureEntityTypeConsistency_ReturnsOk_WhenPathHasDynamicSlug()
    {
        var result = PageValidator.EnsureEntityTypeConsistency("/articles/:id", PageEntityType.Article);
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task EnsureEntityTypeConsistency_ReturnsOk_WhenPathHasMultipleSlugs()
    {
        var result = PageValidator.EnsureEntityTypeConsistency("/blog/:slug/comments/:cid", PageEntityType.Form);
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    [Arguments(PageEntityType.Article)]
    [Arguments(PageEntityType.Form)]
    public async Task EnsureEntityTypeConsistency_ReturnsError_WhenPathHasNoSlug(PageEntityType entityType)
    {
        var result = PageValidator.EnsureEntityTypeConsistency("/home", entityType);
        await Assert.That(result.HasError).IsTrue();
        await Assert.That(result.HasErrorOfType<EntityValidationException>()).IsTrue();
    }

    [Test]
    public async Task EnsureEntityTypeConsistency_ReturnsError_WhenPathIsEmpty()
    {
        var result = PageValidator.EnsureEntityTypeConsistency(string.Empty, PageEntityType.Article);
        await Assert.That(result.HasError).IsTrue();
    }

    [Test]
    public async Task EnsureEntityTypeConsistency_ErrorMessageMentionsEntityTypeAndSlug()
    {
        var result = PageValidator.EnsureEntityTypeConsistency("/home", PageEntityType.Article);
        var message = result.Errors[0].Message ?? "";
        await Assert.That(message).Contains("EntityType");
        await Assert.That(message).Contains("dynamic slug");
    }

    // --- ValidatePatch ---

    [Test]
    public async Task ValidatePatch_ReturnsOk_WhenPatchEmptyAndCurrentConsistent()
    {
        var result = PageValidator.ValidatePatch(Patch(), BuildPage("/home"));
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task ValidatePatch_ReturnsError_WhenPatchEmptyAndCurrentInconsistent()
    {
        // Inconsistent DB state defensive check: current has EntityType but no slug.
        var result = PageValidator.ValidatePatch(Patch(), BuildPage("/home", PageEntityType.Article));
        await Assert.That(result.HasError).IsTrue();
    }

    [Test]
    public async Task ValidatePatch_ReturnsError_WhenPathChangeRemovesSlugButEntityTypeRemains()
    {
        var patch = Patch(new { op = "replace", path = "/Path", value = "/home" });
        var result = PageValidator.ValidatePatch(patch, BuildPage("/articles/:id", PageEntityType.Article));
        await Assert.That(result.HasError).IsTrue();
    }

    [Test]
    public async Task ValidatePatch_ReturnsOk_WhenPathAndEntityTypeClearedTogether()
    {
        var patch = Patch(
            new { op = "replace", path = "/Path", value = "/home" },
            new { op = "replace", path = "/EntityType", value = (string?)null }
        );
        var result = PageValidator.ValidatePatch(patch, BuildPage("/articles/:id", PageEntityType.Article));
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task ValidatePatch_ReturnsError_WhenEntityTypeSetOnPathWithoutSlug()
    {
        var patch = Patch(new { op = "replace", path = "/EntityType", value = "Article" });
        var result = PageValidator.ValidatePatch(patch, BuildPage("/home"));
        await Assert.That(result.HasError).IsTrue();
    }

    [Test]
    public async Task ValidatePatch_ReturnsOk_WhenEntityTypeSetOnPathWithSlug()
    {
        var patch = Patch(new { op = "replace", path = "/EntityType", value = "Article" });
        var result = PageValidator.ValidatePatch(patch, BuildPage("/articles/:id"));
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task ValidatePatch_AcceptsLowercaseEntityTypeValue()
    {
        var patch = Patch(new { op = "replace", path = "/EntityType", value = "article" });
        var result = PageValidator.ValidatePatch(patch, BuildPage("/articles/:id"));
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task ValidatePatch_MatchesPatchPathCaseInsensitively()
    {
        // '/path' lowercase should still match the 'Path' property (OrdinalIgnoreCase).
        var patch = Patch(new { op = "replace", path = "/path", value = "/home" });
        var result = PageValidator.ValidatePatch(patch, BuildPage("/articles/:id", PageEntityType.Article));
        await Assert.That(result.HasError).IsTrue();
    }

    [Test]
    public async Task ValidatePatch_ReturnsOk_WhenPathChangeKeepsSlug()
    {
        var patch = Patch(new { op = "replace", path = "/Path", value = "/products/:id" });
        var result = PageValidator.ValidatePatch(patch, BuildPage("/articles/:id", PageEntityType.Article));
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task ValidatePatch_UsesCurrentPath_WhenPatchValueForPathIsNull()
    {
        // Defensive: patch with null path should fallback to current.Path for the consistency check.
        var patch = Patch(new { op = "replace", path = "/Path", value = (string?)null });
        var result = PageValidator.ValidatePatch(patch, BuildPage("/articles/:id", PageEntityType.Article));
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task ValidatePatch_IgnoresOperationsOnOtherProperties()
    {
        var patch = Patch(new { op = "replace", path = "/Title", value = "ignored" });
        var result = PageValidator.ValidatePatch(patch, BuildPage("/articles/:id", PageEntityType.Article));
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task ValidatePatch_ReturnsOk_WhenPatchIsNotAnArray()
    {
        var patch = JsonSerializer.SerializeToElement(new { not = "an array" });
        var result = PageValidator.ValidatePatch(patch, BuildPage("/home"));
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task ValidatePatch_ReturnsError_WhenPatchIsNotAnArrayAndCurrentInconsistent()
    {
        // When patch can't be parsed, we still enforce the current state.
        var patch = JsonSerializer.SerializeToElement(new { not = "an array" });
        var result = PageValidator.ValidatePatch(patch, BuildPage("/home", PageEntityType.Article));
        await Assert.That(result.HasError).IsTrue();
    }
}
