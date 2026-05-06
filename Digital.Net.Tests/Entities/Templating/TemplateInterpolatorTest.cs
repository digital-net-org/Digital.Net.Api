using Digital.Net.Core.Entities.Attributes;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Core.Entities.Templating;
using Digital.Net.Tests.Core;

namespace Digital.Net.Tests.Entities.Templating;

public class TemplateInterpolatorTest : UnitTest
{
    private class TestSource : Entity
    {
        [Templatable] public string? Title { get; set; }
        [Templatable] public string? Description { get; set; }
        public string? Hidden { get; set; }
    }

    private class TestTarget
    {
        [Templatable] public string? Headline { get; set; }
        [Templatable] public string? Body { get; set; }
        public string? Untouched { get; set; }
        [Templatable] public int IgnoredNonString { get; set; }
    }

    private static IReadOnlyDictionary<string, object> Sources(TestSource source) =>
        new Dictionary<string, object> { ["testsource"] = source };

    [Test]
    public async Task Interpolate_ReplacesSingleToken()
    {
        var sources = Sources(new TestSource { Title = "Hello", Description = "World" });
        var result = TemplateInterpolator.Interpolate("Page: {{ testsource.title }}", sources);
        await Assert.That(result).IsEqualTo("Page: Hello");
    }

    [Test]
    public async Task Interpolate_ReplacesMultipleOccurrences()
    {
        var sources = Sources(new TestSource { Title = "X", Description = "Y" });
        var result = TemplateInterpolator.Interpolate("{{ testsource.title }} - {{ testsource.title }}", sources);
        await Assert.That(result).IsEqualTo("X - X");
    }

    [Test]
    public async Task Interpolate_LeavesUnknownSourcePrefixUntouched()
    {
        var sources = Sources(new TestSource { Title = "X", Description = "Y" });
        var result = TemplateInterpolator.Interpolate("{{ unknown.title }}", sources);
        await Assert.That(result).IsEqualTo("{{ unknown.title }}");
    }

    [Test]
    public async Task Interpolate_LeavesNonWhitelistedFieldUntouched()
    {
        var sources = Sources(new TestSource { Title = "X", Description = "Y", Hidden = "Z" });
        var result = TemplateInterpolator.Interpolate("{{ testsource.hidden }}", sources);
        await Assert.That(result).IsEqualTo("{{ testsource.hidden }}");
    }

    [Test]
    public async Task Interpolate_LeavesUnknownFieldUntouched()
    {
        var sources = Sources(new TestSource { Title = "X", Description = "Y" });
        var result = TemplateInterpolator.Interpolate("{{ testsource.nope }}", sources);
        await Assert.That(result).IsEqualTo("{{ testsource.nope }}");
    }

    [Test]
    public async Task Interpolate_TreatsNullSourceFieldAsEmptyString()
    {
        var sources = Sources(new TestSource { Title = null, Description = "D" });
        var result = TemplateInterpolator.Interpolate(">{{ testsource.title }}<", sources);
        await Assert.That(result).IsEqualTo("><");
    }

    [Test]
    public async Task Interpolate_DoesNotMatchSingleBraces_InJsonContent()
    {
        var sources = Sources(new TestSource { Title = "Hello", Description = null });
        const string template = "{\"@type\":\"Article\",\"headline\":\"{{ testsource.title }}\",\"meta\":{\"k\":\"v\"}}";
        var result = TemplateInterpolator.Interpolate(template, sources);
        await Assert.That(result).IsEqualTo("{\"@type\":\"Article\",\"headline\":\"Hello\",\"meta\":{\"k\":\"v\"}}");
    }

    [Test]
    public async Task Interpolate_NullTemplate_ReturnsNull()
    {
        var result = TemplateInterpolator.Interpolate(null, new Dictionary<string, object>());
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task Interpolate_EmptyTemplate_ReturnsEmpty()
    {
        var result = TemplateInterpolator.Interpolate(string.Empty, new Dictionary<string, object>());
        await Assert.That(result).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Interpolate_ToleratesWhitespace()
    {
        var sources = Sources(new TestSource { Title = "X", Description = "Y" });
        var noSpace = TemplateInterpolator.Interpolate("{{testsource.title}}", sources);
        var lotsOfSpace = TemplateInterpolator.Interpolate("{{   testsource.title   }}", sources);
        await Assert.That(noSpace).IsEqualTo("X");
        await Assert.That(lotsOfSpace).IsEqualTo("X");
    }

    [Test]
    public async Task GetVariables_ReturnsOnlyTemplatableProperties()
    {
        var variables = TemplateInterpolator.GetVariables<TestSource>();
        await Assert.That(variables.Count).IsEqualTo(2);
        await Assert.That(variables.Any(v => v.Field == "Title" && v.Token == "{{ testsource.title }}")).IsTrue();
        await Assert.That(variables.Any(v => v.Field == "Description")).IsTrue();
        await Assert.That(variables.Any(v => v.Field == "Hidden")).IsFalse();
    }

    [Test]
    public async Task HydrateInPlace_RewritesAllTemplatableStrings()
    {
        var sources = Sources(new TestSource { Title = "Foo", Description = "Bar" });
        var target = new TestTarget
        {
            Headline = "{{ testsource.title }}",
            Body = "[{{ testsource.description }}]",
            Untouched = "{{ testsource.title }}",
            IgnoredNonString = 7
        };

        TemplateInterpolator.HydrateInPlace(target, sources);

        await Assert.That(target.Headline).IsEqualTo("Foo");
        await Assert.That(target.Body).IsEqualTo("[Bar]");
        await Assert.That(target.Untouched).IsEqualTo("{{ testsource.title }}");
        await Assert.That(target.IgnoredNonString).IsEqualTo(7);
    }

    [Test]
    public async Task HydrateInPlace_LeavesNullPropertiesUntouched()
    {
        var sources = Sources(new TestSource { Title = "Foo", Description = "Bar" });
        var target = new TestTarget { Headline = null, Body = null };
        TemplateInterpolator.HydrateInPlace(target, sources);
        await Assert.That(target.Headline).IsNull();
        await Assert.That(target.Body).IsNull();
    }
}
