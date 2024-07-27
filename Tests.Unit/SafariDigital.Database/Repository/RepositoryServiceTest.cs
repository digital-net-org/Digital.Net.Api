using SafariDigital.Database.Context;
using SafariDigital.Database.Models.AvatarTable;
using SafariDigital.Database.Models.UserTable;
using SafariDigital.Database.Repository;
using SafariDigital.Database.Sqlite;
using Tests.Core.Base;
using Tests.Core.Factories;

namespace Tests.Unit.SafariDigital.Database.Repository;

public class RepositoryServiceTest : UnitTest
{
    private readonly SafariDigitalContext _context = new SqliteDatabase().Context;
    private IRepositoryService<Avatar> _avatarRepository;
    private IRepositoryService<User> _userRepository;

    [Fact]
    public void GetById_WithInt_ShouldReturnEntity()
    {
        // Arrange
        var avatar = SetupAvatar();

        // Act
        var avatarById = _avatarRepository.GetById(avatar.Id);

        // Assert
        Assert.NotNull(avatarById);
        Assert.Equal(avatar.Id, avatarById.Value!.Id);
    }

    [Fact]
    public async void GetByIdAsync_WithInt_ShouldReturnEntity()
    {
        // Arrange
        var avatar = SetupAvatar();

        // Act
        var avatarById = await _avatarRepository.GetByIdAsync(avatar.Id);

        // Assert
        Assert.NotNull(avatarById);
        Assert.Equal(avatar.Id, avatarById.Value!.Id);
    }

    [Fact]
    public void GetById_WithGuid_ShouldReturnEntity()
    {
        // Arrange
        var user = Setup();

        // Act
        var userById = _userRepository.GetById(user.Id);

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
        var userById = await _userRepository.GetByIdAsync(user.Id);

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
        var userById = _userRepository.GetFirstOrDefault(u => u.Username == user.Username);

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
        var userById = await _userRepository.GetFirstOrDefaultAsync(u => u.Username == user.Username);

        // Assert
        Assert.NotNull(userById);
        Assert.Equal(user.Username, userById.Value!.Username);
    }

    private User Setup()
    {
        var user = UserFactory.CreateUser();
        _userRepository = new RepositoryService<User>(new Repository<User>(_context));
        _userRepository.Create(user);
        _userRepository.Save();
        return _userRepository.Get(u => u.Username == user.Username).Value!.FirstOrDefault()!;
    }

    private Avatar SetupAvatar()
    {
        var avatar = AvatarFactory.CreateAvatar();
        _avatarRepository = new RepositoryService<Avatar>(new Repository<Avatar>(_context));
        _avatarRepository.Create(avatar);
        _avatarRepository.Save();
        return _avatarRepository.GetFirstOrDefault(a => a.Id != 0).Value!;
    }
}