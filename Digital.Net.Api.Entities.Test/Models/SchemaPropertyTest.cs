using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Api.Entities.Attributes;
using Digital.Net.Api.Entities.Models;
using Digital.Net.Api.TestUtilities;

namespace Digital.Net.Api.Entities.Test.Models;

public class SchemaPropertyTest : UnitTest
{
    private class TestEntity : EntityId
    {
        [Column("required_property"), DataFlag("test_flag"), Required, ReadOnly]
        public string RequiredProperty { get; set; }
    }

    [Fact]
    public void SchemaProperty_SetsPropertiesCorrectly()
    {
        var propertyInfo = typeof(TestEntity).GetProperty("RequiredProperty");
        var schemaProperty = new SchemaProperty<TestEntity>(propertyInfo!);
        Assert.Equal("RequiredProperty", schemaProperty.Name);
        Assert.Equal("required_property", schemaProperty.Path);
        Assert.Equal(propertyInfo!.PropertyType.Name, schemaProperty.Type);
        Assert.Equal("test_flag", schemaProperty.DataFlag);
        Assert.Null(schemaProperty.RegexValidation);
        Assert.True(schemaProperty.IsRequired);
        Assert.True(schemaProperty.IsReadOnly);
        Assert.False(schemaProperty.IsSecret);
        Assert.False(schemaProperty.IsUnique);
        Assert.False(schemaProperty.IsIdentity);
        Assert.False(schemaProperty.IsForeignKey);
        Assert.Null(schemaProperty.MaxLength);
    }
}