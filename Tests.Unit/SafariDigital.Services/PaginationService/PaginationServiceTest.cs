using System.Linq.Expressions;
using Moq;
using SafariDigital.Database.Models.UserTable;
using SafariDigital.Database.Repository;
using SafariDigital.DataIdentities.Pagination.User;
using SafariDigital.Services.PaginationService;
using Tests.Core.Factories;

namespace Tests.Unit.SafariDigital.Services.PaginationService;

public class PaginationServiceTest
{
    private readonly UserPaginationService _paginationService;
    private readonly Mock<IRepository<User>> _repository;

    public PaginationServiceTest()
    {
        _repository = new Mock<IRepository<User>>();
        _paginationService = new UserPaginationService(_repository.Object);
    }

    private void SetupRepositoryService(List<User> result) =>
        _repository.Setup(x => x.Get(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(result.AsQueryable());

    [Fact]
    public void Get_WithValidIndex_ShouldReturnPaginationResult()
    {
        // Arrange
        const int total = 10;
        const int index = 1;
        const int size = 5;
        var query = new UserQuery { Index = index, Size = size };
        SetupRepositoryService(UserFactory.CreateManyUsers(total));

        // Act
        var result = _paginationService.Get(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(total, result.Total);
        Assert.Equal(size, result.Size);
        Assert.Equal(index, result.Index);
        Assert.Equal(size < total ? size : total, result.Result.Count);
    }

    [Fact]
    public void Get_WithInvalidIndex_ShouldReturnCorrectedPaginationResult()
    {
        // Arrange
        const int total = PaginationUtils.MaxSize + 1;
        const int index = -1;
        var query = new UserQuery { Index = index, Size = total };
        SetupRepositoryService(UserFactory.CreateManyUsers(total));

        // Act
        var result = _paginationService.Get(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(total, result.Total);
        Assert.Equal(PaginationUtils.DefaultSize, result.Size);
        Assert.Equal(PaginationUtils.DefaultSize, result.Result.Count);
        Assert.Equal(PaginationUtils.DefaultIndex, result.Index);
    }

    [Fact]
    public void Get_IndexInSecondPage_ShouldReturnCorrectItems()
    {
        // Arrange
        const int total = 10;
        const int index = 2;
        const int size = 5;
        var users = UserFactory.CreateManyUsers(total);
        var query = new UserQuery { Index = index, Size = size, OrderBy = "CreatedAt" };
        foreach (var user in users)
        {
            var i = users.IndexOf(user) + 1;
            user.Username = $"User{i}";
            user.CreatedAt = DateTime.Now.AddDays(-total + i);
        }

        SetupRepositoryService(users);

        // Act
        var result = _paginationService.Get(query);

        // Assert
        Assert.Equal("User6", result.Result.First().Username);
    }

    [Fact]
    public void Get_InvalidOrder_ShouldReturnError()
    {
        // Arrange
        const int total = 10;
        var query = new UserQuery { OrderBy = "Lol" };
        SetupRepositoryService(UserFactory.CreateManyUsers(total));

        // Act
        var result = _paginationService.Get(query);

        // Assert
        Assert.True(result.HasError);
        Assert.NotEmpty(result.Error);
    }
}