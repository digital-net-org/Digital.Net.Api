using Digital.Lib.Net.Core.Models;
using SafariDigital.Api.Dto.Entities;
using SafariDigital.Data.Models.Users;

namespace Tests.SafariDigital.Database.Models.Dto.Users;

public class UserTest
{
    [Fact]
    public void UserModel_ReturnsValidModel()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "username",
            Email = "email",
            Login = "email",
            Password = "password",
            Role = UserRole.Admin,
            Avatar = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var model = Mapper.MapFromConstructor<User, UserModel>(user);
        Assert.NotNull(model);
        Assert.IsType<UserModel>(model);
        Assert.Equal(user.Id, model.Id);
        Assert.Equal(user.Username, model.Username);
        Assert.Equal(user.Email, model.Email);
        Assert.Equal(user.Login, model.Login);
        Assert.Equal(user.Role, model.Role);
        Assert.Equal(user.IsActive, model.IsActive);
        Assert.Equal(user.CreatedAt, model.CreatedAt);
        Assert.Equal(user.UpdatedAt, model.UpdatedAt);
    }
}