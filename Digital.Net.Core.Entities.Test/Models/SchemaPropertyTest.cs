using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Core.Entities.Attributes;
using Digital.Net.Core.Entities.Exceptions;
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

        [Required]
        [RegexValidation(@"^[a-z]{6,}$")]
        public string ValidatedProperty { get; set; }

        [OneOf("alpha", "beta", "gamma")]
        public string? OneOfProperty { get; set; }

        [ForeignKey("Foreign")]
        public Guid ForeignId { get; set; }

        public TestEnum EnumProperty { get; set; }

        public TestEnum? NullableEnumProperty { get; set; }
    }

    private static SchemaProperty<TestEntity> SchemaFor(string propertyName)
    {
        var info = typeof(TestEntity).GetProperty(propertyName)
                   ?? throw new InvalidOperationException($"Missing test property '{propertyName}'.");
        return new SchemaProperty<TestEntity>(info);
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
        await Assert.That(schemaProperty.EnumValues).IsEquivalentTo(["Alpha", "Beta", "Gamma"]);
    }

    [Test]
    public async Task SchemaProperty_DetectsNullableEnumType()
    {
        var propertyInfo = typeof(TestEntity).GetProperty("NullableEnumProperty");
        var schemaProperty = new SchemaProperty<TestEntity>(propertyInfo!);
        await Assert.That(schemaProperty.Type).IsEqualTo("Enum");
        await Assert.That(schemaProperty.EnumValues).IsEquivalentTo(["Alpha", "Beta", "Gamma"]);
    }

    [Test]
    public async Task Validate_DoesNotThrow_WhenValueIsNull()
    {
        var schema = SchemaFor("ValidatedProperty");
        await Assert.That(() => schema.Validate(null, "ValidatedProperty")).ThrowsNothing();
    }

    [Test]
    public async Task Validate_DoesNotThrow_WhenRequiredStringHasValidValue()
    {
        var schema = SchemaFor("ValidatedProperty");
        await Assert.That(() => schema.Validate("validname", "ValidatedProperty")).ThrowsNothing();
    }

    [Test]
    public async Task Validate_Throws_WhenRequiredStringIsEmpty()
    {
        var schema = SchemaFor("ValidatedProperty");
        await Assert.ThrowsAsync<EntityValidationException>(async () =>
        {
            schema.Validate("", "ValidatedProperty");
            await Task.CompletedTask;
        });
    }

    [Test]
    public async Task Validate_Throws_WhenRegexFails()
    {
        var schema = SchemaFor("ValidatedProperty");
        await Assert.ThrowsAsync<EntityValidationException>(async () =>
        {
            schema.Validate("ab", "ValidatedProperty");
            await Task.CompletedTask;
        });
    }

    [Test]
    public async Task Validate_DoesNotThrow_WhenOneOfValueIsKnown()
    {
        var schema = SchemaFor("OneOfProperty");
        await Assert.That(() => schema.Validate("alpha", "OneOfProperty")).ThrowsNothing();
    }

    [Test]
    public async Task Validate_Throws_WhenOneOfValueIsUnknown()
    {
        var schema = SchemaFor("OneOfProperty");
        await Assert.ThrowsAsync<EntityValidationException>(async () =>
        {
            schema.Validate("delta", "OneOfProperty");
            await Task.CompletedTask;
        });
    }

    [Test]
    public async Task Validate_DoesNotThrow_WhenIdentityHasEmptyGuid()
    {
        var schema = SchemaFor("Id");
        await Assert.That(() => schema.Validate(Guid.Empty, "Id")).ThrowsNothing();
    }

    [Test]
    public async Task Validate_DoesNotThrow_WhenForeignKeyIsEmptyGuid()
    {
        var schema = SchemaFor("ForeignId");
        await Assert.That(() => schema.Validate(Guid.Empty, "ForeignId")).ThrowsNothing();
    }

    [Test]
    public async Task Validate_DoesNotThrow_WhenCreatedAtIsMinValue()
    {
        var schema = SchemaFor("CreatedAt");
        await Assert.That(() => schema.Validate(DateTime.MinValue, "CreatedAt")).ThrowsNothing();
    }

    [Test]
    public async Task Validate_DoesNotThrow_WhenUpdatedAtIsMinValue()
    {
        var schema = SchemaFor("UpdatedAt");
        await Assert.That(() => schema.Validate(DateTime.MinValue, "UpdatedAt")).ThrowsNothing();
    }

    [Test]
    public async Task Validate_Throws_WhenIdentityHasNonDefaultValue()
    {
        var schema = SchemaFor("Id");
        await Assert.ThrowsAsync<EntityValidationException>(async () =>
        {
            schema.Validate(Guid.NewGuid(), "Id");
            await Task.CompletedTask;
        });
    }

    [Test]
    public async Task ValidateMutation_DoesNotThrow_WhenRegularPropertyHasValidValue()
    {
        var schema = SchemaFor("ValidatedProperty");
        await Assert.That(() => schema.ValidateMutation("validname", "ValidatedProperty")).ThrowsNothing();
    }

    [Test]
    public async Task ValidateMutation_Throws_WhenSchemaIsReadOnly()
    {
        var schema = SchemaFor("RequiredProperty");
        await Assert.ThrowsAsync<EntityValidationException>(async () =>
        {
            schema.ValidateMutation("anything", "RequiredProperty");
            await Task.CompletedTask;
        });
    }

    [Test]
    public async Task ValidateMutation_Throws_WhenSchemaIsIdentity()
    {
        var schema = SchemaFor("Id");
        await Assert.ThrowsAsync<EntityValidationException>(async () =>
        {
            schema.ValidateMutation(Guid.Empty, "Id");
            await Task.CompletedTask;
        });
    }

    [Test]
    public async Task ValidateMutation_PropagatesValidateError_WhenValueIsInvalid()
    {
        var schema = SchemaFor("ValidatedProperty");
        await Assert.ThrowsAsync<EntityValidationException>(async () =>
        {
            schema.ValidateMutation(string.Empty, "ValidatedProperty");
            await Task.CompletedTask;
        });
    }
}
