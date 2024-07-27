using SafariDigital.DataIdentities.Models.User;
using Tests.Core.Factories;

namespace Tests.Unit.SafariDigital.Database.Models.UserTable;

public class UserTest
{
    [Fact]
    public void GetModel_ReturnsCorrectModelType()
    {
        // Arrange
        var user = UserFactory.CreateUser();

        // Act
        var userPublicModel = user.GetModel<UserPublicModel>();

        // Assert
        Assert.NotNull(userPublicModel);
        Assert.IsType<UserPublicModel>(userPublicModel);
        Assert.Equal(user.Id, userPublicModel.Id);
        Assert.Equal(user.Username, userPublicModel.Username);
        Assert.Equal(user.Email, userPublicModel.Email);
        Assert.Equal(user.Role, userPublicModel.Role);
        if (user.Avatar != null)
        {
            Assert.NotNull(userPublicModel.Avatar);
            Assert.Equal(user.Avatar.Id, userPublicModel.Avatar.Id);
        }
        else
        {
            Assert.Null(userPublicModel.Avatar);
        }

        Assert.Equal(user.IsActive, userPublicModel.IsActive);
        Assert.Equal(user.CreatedAt, userPublicModel.CreatedAt);
        Assert.Equal(user.UpdatedAt, userPublicModel.UpdatedAt);
    }
}