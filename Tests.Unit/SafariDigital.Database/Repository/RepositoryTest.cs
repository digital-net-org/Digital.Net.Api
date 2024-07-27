using SafariDigital.Database.Context;
using SafariDigital.Database.Models.UserTable;
using SafariDigital.Database.Repository;
using SafariDigital.Database.Sqlite;
using Tests.Core.Base;
using Tests.Core.Factories;

namespace Tests.Unit.SafariDigital.Database.Repository;

public class RepositoryTest : UnitTest
{
    private readonly SafariDigitalContext _context = new SqliteDatabase().Context;
    private IRepository<User> _repository;

    [Fact]
    public void GetById_ShouldReturnUser()
    {
        // Arrange
        var user = Setup();

        // Act
        var userById = _repository.GetByPrimaryKey(user.Id);

        // Assert
        Assert.NotNull(userById);
        Assert.Equal(user.Id, userById.Id);
    }

    [Fact]
    public async void GetByIdAsync_ShouldReturnUser()
    {
        // Arrange
        var user = Setup();

        // Act
        var userById = await _repository.GetByPrimaryKeyAsync(user.Id);

        // Assert
        Assert.NotNull(userById);
        Assert.Equal(user.Id, userById.Id);
    }

    [Fact]
    public async void GetById_WithStringId_ShouldReturnUser()
    {
        // Arrange
        var user = Setup();

        // Act
        var userById = _repository.GetByPrimaryKey(user.Id.ToString());

        // Assert
        Assert.NotNull(userById);
        Assert.Equal(user.Id, userById.Id);
    }

    [Fact]
    public async void GetByIdAsync_WithStringId_ShouldReturnUser()
    {
        // Arrange
        var user = Setup();

        // Act
        var userById = await _repository.GetByPrimaryKeyAsync(user.Id.ToString());

        // Assert
        Assert.NotNull(userById);
        Assert.Equal(user.Id, userById.Id);
    }

    [Fact]
    public void Create_ShouldCreateEntity()
    {
        // Arrange
        _repository = new Repository<User>(_context);
        var user = UserFactory.CreateUser();

        // Act
        _repository.Create(user);
        _repository.Save();
        var createdUser = _repository.Get(u => u.Username == user.Username).FirstOrDefault();

        // Assert
        Assert.NotNull(createdUser);
        Assert.Equal(user.Username, createdUser.Username);
        Assert.True(createdUser?.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async void CreateAsync_ShouldCreateEntity()
    {
        // Arrange
        _repository = new Repository<User>(_context);
        var user = UserFactory.CreateUser();

        // Act
        await _repository.CreateAsync(user);
        await _repository.SaveAsync();
        var createdUser = _repository.Get(u => u.Username == user.Username).FirstOrDefault();

        // Assert
        Assert.NotNull(createdUser);
        Assert.Equal(user.Username, createdUser.Username);
        Assert.True(createdUser?.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Update_ShouldUpdateUser()
    {
        // Arrange
        var user = Setup();
        user.Username = "John Arbuckle";

        // Act
        _repository.Update(user);
        _repository.Save();
        var updatedUser = _repository.Get(u => u.Username == user.Username).FirstOrDefault()!;

        // Assert
        Assert.Equal(user.Username, updatedUser.Username);
        Assert.True(updatedUser.UpdatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Delete_ShouldDeleteUser()
    {
        // Arrange
        var user = Setup();

        // Act
        var userToDelete = _repository.Get(u => u.Username == user.Username).FirstOrDefault();
        _repository.Delete(userToDelete!);
        _repository.Save();

        // Assert
        var deletedUser = _repository.Get(u => u.Username == user.Username).FirstOrDefault();
        Assert.True(deletedUser is null);
    }

    private User Setup()
    {
        var user = UserFactory.CreateUser();
        _repository = new Repository<User>(_context);
        _repository.Create(user);
        _repository.Save();
        return _repository.Get(u => u.Username == user.Username).FirstOrDefault()!;
    }
}