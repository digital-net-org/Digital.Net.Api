using Digital.Net.Core.String;
using Digital.Net.Tests.Core;

namespace Digital.Net.Core.Test.String;

public class RegularExpressionsTests : UnitTest
{
    [Test]
    [Arguments("username", true)]
    [Arguments("user_name", true)]
    [Arguments("user-name", true)]
    [Arguments("username123", true)]
    [Arguments("123456", true)]
    [Arguments("____", false)]
    [Arguments("abc", false)]
    [Arguments("user name", false)]
    [Arguments("username!@#", false)]
    [Arguments("thisusernameistoolongforthisfield", false)]
    public async Task Username_Regex_Should_Validate_Correctly(string username, bool expected)
    {
        var result = RegularExpressions.Username.IsMatch(username);
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("test@test.com", true)]
    [Arguments("test.test@test.com", true)]
    [Arguments("test_test@test.com", true)]
    [Arguments("test-test@test.com", true)]
    [Arguments("test+test@test.com", true)]
    [Arguments("test@test.co.uk", true)]
    [Arguments("test@test", false)]
    [Arguments("test.com", false)]
    [Arguments("@test.com", false)]
    public async Task Email_Regex_Should_Validate_Correctly(string email, bool expected)
    {
        var result = RegularExpressions.Email.IsMatch(email);
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("Password123!", true)]
    [Arguments("aA1!aaaaaaaaa", true)]
    [Arguments("password123!", false)]
    [Arguments("PASSWORD123!", false)]
    [Arguments("Password!!!", false)]
    [Arguments("Password123", false)]
    [Arguments("Short1!", false)]
    public async Task Password_Regex_Should_Validate_Correctly(string password, bool expected)
    {
        var result = RegularExpressions.Password.IsMatch(password);
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("pascalCase", true)]
    [Arguments("PascalCase", true)]
    [Arguments("PASCAL", true)]
    [Arguments("Pascal", false)]
    [Arguments("pascal", false)]
    [Arguments("P", false)]
    public async Task PascalCase_Regex_Should_Validate_Correctly(string pascalCase, bool expected)
    {
        var result = RegularExpressions.PascalCase().IsMatch(pascalCase);
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("valid-key-1", true)]
    [Arguments("ValidKey", true)]
    [Arguments("key 123", true)]
    [Arguments("key_123", true)]
    [Arguments("key-123", true)]
    [Arguments("", false)]
    [Arguments("key!@#", false)]
    [Arguments("key.invalid", false)]
    public async Task ApiKeyName_Regex_Should_Validate_Correctly(string name, bool expected)
    {
        var result = RegularExpressions.ApiKeyName.IsMatch(name);
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("object.name", true)]
    [Arguments("object.name.too", true)]
    [Arguments(".object.name", true)]
    [Arguments("objectname", false)]
    [Arguments(".objectname", false)]
    public async Task ObjectName_Regex_Should_Validate_Correctly(string objectName, bool expected)
    {
        var result = RegularExpressions.ObjectName().IsMatch(objectName);
        await Assert.That(result).IsEqualTo(expected);
    }
}