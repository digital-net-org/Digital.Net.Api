using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Core.Entities.Attributes;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Tests.Core;

namespace Digital.Net.Core.Entities.Test.Models;

public class SchemaPropertyTest : UnitTest
{
    private class TestEntity : Entity
    {
        [Column("required_property"), DataFlag("test_flag"), Required, ReadOnly]
        public string RequiredProperty { get; set; }
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
    }
}