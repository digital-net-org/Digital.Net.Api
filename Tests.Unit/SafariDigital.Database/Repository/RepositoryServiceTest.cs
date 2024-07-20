using SafariDigital.Database.Context;
using SafariDigital.Database.Models.User;
using SafariDigital.Database.Repository;
using Tests.Core.Base;
using Tests.Core.Factories;
using Tests.Core.Integration;

namespace Tests.Unit.SafariDigital.Database.Repository;

public class RepositoryServiceTest : UnitTest
{
    private readonly SafariDigitalContext _context = new SqliteDatabase().Context;
    private IRepositoryService<User> _repository;

    // TODO: To replace with int based repository once available
    // [Fact]
    // public void GetById_WithInt_ShouldReturnEntity()
    // {
    //     // Arrange
    //     var user = Setup();
    //
    //     // Act
    //     var userById = _repository.GetById(user.Id);
    //
    //     // Assert
    //     Assert.NotNull(userById);
    //     Assert.Equal(user.Id, userById.Value!.Id);
    // }
    //
    // [Fact]
    // public async void GetByIdAsync_WithInt_ShouldReturnEntity()
    // {
    //     // Arrange
    //     var user = Setup();
    //
    //     // Act
    //     var userById = await _repository.GetByIdAsync(user.Id);
    //
    //     // Assert
    //     Assert.NotNull(userById);
    //     Assert.Equal(user.Id, userById.Value!.Id);
    // }

    [Fact]
    public void GetById_WithGuid_ShouldReturnEntity()
    {
        // Arrange
        var user = Setup();

        // Act
        var userById = _repository.GetById(user.Id);

        // Assert
        Assert.NotNull(userById);
        Assert.Equal(user.Id, userById.Value!.Id);
    }

    [Fact]
    public async void GetByIdAsync_WithGuid_ShouldReturnEntity()
    {
        // Arrange
        var user = Setup();

        // Act
        var userById = await _repository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(userById);
        Assert.Equal(user.Id, userById.Value!.Id);
    }

    [Fact]
    public void GetFirstOrDefault_ShouldReturnEntity()
    {
        // Arrange
        var user = Setup();

        // Act
        var userById = _repository.GetFirstOrDefault(u => u.Username == user.Username);

        // Assert
        Assert.NotNull(userById);
        Assert.Equal(user.Username, userById.Value!.Username);
    }

    [Fact]
    public async void GetFirstOrDefaultAsync_ShouldReturnEntity()
    {
        // Arrange
        var user = Setup();

        // Act
        var userById = await _repository.GetFirstOrDefaultAsync(u => u.Username == user.Username);

        // Assert
        Assert.NotNull(userById);
        Assert.Equal(user.Username, userById.Value!.Username);
    }

    private User Setup()
    {
        var user = UserFactory.CreateUser();
        _repository = new RepositoryService<User>(new Repository<User>(_context));
        _repository.Create(user);
        _repository.Save();
        return _repository.Get(u => u.Username == user.Username).Value!.FirstOrDefault()!;
    }
}