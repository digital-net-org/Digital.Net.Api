using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Core.Entities.Attributes;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Tests.Core;

namespace Digital.Net.Core.Entities.Test.Models;

public class SchemaPropertyTest : UnitTest
{
    private enum TestEnum
    {
        Alpha,
        Beta,
        Gamma
    }

    private class TestEntity : Entity
    {
        [Column("required_property"), DataFlag("test_flag"), Required, ReadOnly]
        public string RequiredProperty { get; set; }

        [RegexValidation(@"^[a-z]+$")]
        public string RegexProperty { get; set; }

        public TestEnum EnumProperty { get; set; }

        public TestEnum? NullableEnumProperty { get; set; }
    }

    [Test]
    public async Task SchemaProperty_SetsPropertiesCorrectly()
    {
        var propertyInfo = typeof(TestEntity).GetProperty("RequiredProperty");
        var schemaProperty = new SchemaProperty<TestEntity>(propertyInfo!);
        await Assert.That(schemaProperty.Name).IsEqualTo("RequiredProperty");
        await Assert.That(schemaProperty.Path).IsEqualTo("required_property");
        await Assert.That(schemaProperty.Type).IsEqualTo(propertyInfo!.PropertyType.Name);
        await Assert.That(schemaProperty.DataFlag).IsEqualTo("test_flag");
        await Assert.That(schemaProperty.RegexValidation).IsNull();
        await Assert.That(schemaProperty.IsRequired).IsTrue();
        await Assert.That(schemaProperty.IsReadOnly).IsTrue();
        await Assert.That(schemaProperty.IsSecret).IsFalse();
        await Assert.That(schemaProperty.IsUnique).IsFalse();
        await Assert.That(schemaProperty.IsIdentity).IsFalse();
        await Assert.That(schemaProperty.IsForeignKey).IsFalse();
        await Assert.That(schemaProperty.MaxLength).IsNull();
        await Assert.That(schemaProperty.EnumValues).IsNull();
    }

    [Test]
    public async Task SchemaProperty_ExposesRegexPatternAsString()
    {
        var propertyInfo = typeof(TestEntity).GetProperty("RegexProperty");
        var schemaProperty = new SchemaProperty<TestEntity>(propertyInfo!);
        await Assert.That(schemaProperty.RegexValidation).IsEqualTo(@"^[a-z]+$");
    }

    [Test]
    public async Task SchemaProperty_DetectsEnumType()
    {
        var propertyInfo = typeof(TestEntity).GetProperty("EnumProperty");
        var schemaProperty = new SchemaProperty<TestEntity>(propertyInfo!);
        await Assert.That(schemaProperty.Type).IsEqualTo("Enum");
        await Assert.That(schemaProperty.EnumValues).IsEquivalentTo(new[] { "Alpha", "Beta", "Gamma" });
    }

    [Test]
    public async Task SchemaProperty_DetectsNullableEnumType()
    {
        var propertyInfo = typeof(TestEntity).GetProperty("NullableEnumProperty");
        var schemaProperty = new SchemaProperty<TestEntity>(propertyInfo!);
        await Assert.That(schemaProperty.Type).IsEqualTo("Enum");
        await Assert.That(schemaProperty.EnumValues).IsEquivalentTo(new[] { "Alpha", "Beta", "Gamma" });
    }
}
