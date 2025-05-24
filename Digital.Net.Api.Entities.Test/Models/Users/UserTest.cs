using Digital.Net.Api.Core.Models;
using Digital.Net.Api.Entities.Models.Users;

namespace Digital.Net.Api.Entities.Test.Models.Users;

public class UserTest
{
    public static readonly User TestUser = new()
    {
        Id = Guid.NewGuid(),
        Username = "username",
        Email = "email",
        Login = "email",
        Password = "password",
        Avatar = null,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    [Fact]
    public void UserModel_ReturnsValidModel()
    {

        var dto = Mapper.MapFromConstructor<User, UserDto>(TestUser);
        Assert.NotNull(dto);
        Assert.IsType<UserDto>(dto);
        Assert.Equal(TestUser.Id, dto.Id);
        Assert.Equal(TestUser.Username, dto.Username);
        Assert.Equal(TestUser.Email, dto.Email);
        Assert.Equal(TestUser.Login, dto.Login);
        Assert.Equal(TestUser.IsActive, dto.IsActive);
        Assert.Equal(TestUser.CreatedAt, dto.CreatedAt);
        Assert.Equal(TestUser.UpdatedAt, dto.UpdatedAt);
    }
}