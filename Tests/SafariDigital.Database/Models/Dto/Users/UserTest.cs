using Digital.Net.Core.Models;
using SafariDigital.Data.Models.Database.Users;
using SafariDigital.Data.Models.Dto.Users;

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
            Password = "password",
            Role = EUserRole.Admin,
            Avatar = null,
            IsActive = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        var model = Mapper.MapFromConstructor<User, UserModel>(user);
        Assert.NotNull(model);
        Assert.IsType<UserModel>(model);
        Assert.Equal(user.Id, model.Id);
        Assert.Equal(user.Username, model.Username);
        Assert.Equal(user.Email, model.Email);
        Assert.Equal(user.Role, model.Role);
        Assert.Equal(user.IsActive, model.IsActive);
        Assert.Equal(user.CreatedAt, model.CreatedAt);
        Assert.Equal(user.UpdatedAt, model.UpdatedAt);
    }
}