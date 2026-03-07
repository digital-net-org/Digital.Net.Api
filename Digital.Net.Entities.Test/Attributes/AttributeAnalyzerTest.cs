using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Entities.Attributes;
using Digital.Net.Entities.Models;
using Digital.Net.Tests.Core;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Entities.Test.Attributes;

public class AttributeAnalyzerTest : UnitTest
{
    private enum ETestFlag
    {
        TestValue
    }

    [Index(nameof(UniqueProperty), IsUnique = true)]
    private class TestEntity : Entity
    {
        [Required]
        public string RequiredProperty { get; set; }

        public string UniqueProperty { get; set; }

        [MaxLength(50)]
        public string MaxLengthProperty { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdentityProperty { get; set; }

        [ForeignKey("ForeignKeyProperty")]
        public int ForeignKeyProperty { get; set; }

        [Column("form_property"), DataFlag("test_flag"), ReadOnly]
        public DateTime FormProperty { get; set; }

        [Secret, RegexValidation("^[a-zA-Z0-9.'@_-]{6,24}$")]
        public string SecretProperty { get; set; }
    }

    [Test]
    public async Task IsRequired_ReturnsTrue_ForRequiredProperty() =>
        await Assert.That(AttributeAnalyzer<TestEntity>.IsRequired("RequiredProperty")).IsTrue();

    [Test]
    public async Task IsRequired_ReturnsFalse_ForNonRequiredProperty() =>
        await Assert.That(AttributeAnalyzer<TestEntity>.IsRequired("UniqueProperty")).IsFalse();

    [Test]
    public async Task IsUnique_ReturnsTrue_ForUniqueProperty() =>
        await Assert.That(AttributeAnalyzer<TestEntity>.IsUnique("UniqueProperty")).IsTrue();

    [Test]
    public async Task IsUnique_ReturnsFalse_ForNonUniqueProperty() =>
        await Assert.That(AttributeAnalyzer<TestEntity>.IsUnique("RequiredProperty")).IsFalse();

    [Test]
    public async Task MaxLength_ReturnsCorrectLength_ForMaxLengthProperty() =>
        await Assert.That(AttributeAnalyzer<TestEntity>.MaxLength("MaxLengthProperty")).IsEqualTo(50);

    [Test]
    public async Task MaxLength_ReturnsZero_ForNonMaxLengthProperty() =>
        await Assert.That(AttributeAnalyzer<TestEntity>.MaxLength("RequiredProperty")).IsEqualTo(0);

    [Test]
    public async Task IsIdentity_ReturnsTrue_ForIdentityProperty() =>
        await Assert.That(AttributeAnalyzer<TestEntity>.IsIdentity("IdentityProperty")).IsTrue();

    [Test]
    public async Task IsIdentity_ReturnsFalse_ForNonIdentityProperty() =>
        await Assert.That(AttributeAnalyzer<TestEntity>.IsIdentity("RequiredProperty")).IsFalse();

    [Test]
    public async Task IsForeignKey_ReturnsTrue_ForForeignKeyProperty() =>
        await Assert.That(AttributeAnalyzer<TestEntity>.IsForeignKey("ForeignKeyProperty")).IsTrue();

    [Test]
    public async Task IsForeignKey_ReturnsFalse_ForNonForeignKeyProperty() =>
        await Assert.That(AttributeAnalyzer<TestEntity>.IsForeignKey("RequiredProperty")).IsFalse();

    [Test]
    public async Task IsSecret_ReturnsTrue_ForSecretProperty() =>
        await Assert.That(AttributeAnalyzer<TestEntity>.IsSecret("SecretProperty")).IsTrue();

    [Test]
    public async Task GetPath_ReturnsCorrectPath_ForColumnProperty() =>
        await Assert.That(AttributeAnalyzer<TestEntity>.GetPath("FormProperty")).IsEqualTo("form_property");

    [Test]
    public async Task GetPath_ReturnsPropertyName_ForNonColumnProperty() =>
        await Assert.That(AttributeAnalyzer<TestEntity>.GetPath("RequiredProperty")).IsEqualTo("RequiredProperty");

    [Test]
    public async Task GetDataFlag_ReturnsCorrectFlag_ForFormProperty() =>
        await Assert.That(AttributeAnalyzer<TestEntity>.GetDataFlag("FormProperty")).IsEqualTo("test_flag");

    [Test]
    public async Task GetDataFlag_ReturnsNull_ForNonFormProperty() =>
        await Assert.That(AttributeAnalyzer<TestEntity>.GetDataFlag("RequiredProperty")).IsNull();

    [Test]
    public async Task IsReadOnly_ReturnsFalse_ForNonReadOnlyProperty() =>
        await Assert.That(AttributeAnalyzer<TestEntity>.IsReadOnly("RequiredProperty")).IsFalse();

    [Test]
    public async Task IsReadOnly_ReturnsFalse_ForReadOnlyProperty() =>
        await Assert.That(AttributeAnalyzer<TestEntity>.IsReadOnly("FormProperty")).IsTrue();

    [Test]
    public async Task GetRegex_ReturnsNull_ForNonRegexProperty() =>
        await Assert.That(AttributeAnalyzer<TestEntity>.GetRegex("RequiredProperty")).IsNull();

    [Test]
    public async Task GetRegex_ReturnsNotNull_ForRegexProperty() =>
        await Assert.That(AttributeAnalyzer<TestEntity>.GetRegex("SecretProperty")).IsNotNull();
}