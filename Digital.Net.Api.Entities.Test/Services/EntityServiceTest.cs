using System.Linq.Expressions;
using Digital.Net.Api.Core.Exceptions;
using Digital.Net.Api.Core.Random;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Exceptions;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.Entities.Services;
using Digital.Net.Api.TestUtilities;
using Digital.Net.Api.TestUtilities.Data;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Data.Sqlite;

namespace Digital.Net.Api.Entities.Test.Services;

public class EntityServiceTest : UnitTest, IDisposable
{
    private static User GetTestUser() => new()
    {
            Username = Randomizer.GenerateRandomString(),
            Login = Randomizer.GenerateRandomString(),
            Password = Randomizer.GenerateRandomString(),
            Email = Randomizer.GenerateRandomEmail()
    };

    private static JsonPatchDocument<User> CreateUserPatch<T>(Expression<Func<User, T>> path, T value)
    {
        var patch = new JsonPatchDocument<User>();
        patch.Replace(path, value);
        return patch;
    }

    private readonly SqliteConnection _connection;
    private readonly Repository<User, DigitalContext> _userRepository;
    private readonly IEntityService<User, DigitalContext> _userService;

    public EntityServiceTest()
    {
        _connection = SqliteInMemoryHelper.GetConnection();
        var context = _connection.CreateContext<DigitalContext>();
        _userRepository = new Repository<User, DigitalContext>(context);
        _userService = new EntityService<User, DigitalContext>(_userRepository);
    }

    [Fact]
    public void GetSchema_ReturnsCorrectSchema_WhenEntityHasProperties() =>
        Assert.Equal("Username", _userService.GetSchema()[0].Name);

    [Fact]
    public async Task Patch_ReturnsMappedModel_WhenQueryIsValid()
    {
        var user = await _userRepository.CreateAndSaveAsync(GetTestUser());
        var patch = CreateUserPatch(u => u.Username, "NewUsername");

        var result = await _userService.Patch(patch, user.Id);
        var updatedUser = await _userRepository.GetByIdAsync(user.Id);
        Assert.NotNull(result);
        Assert.Equal("NewUsername", updatedUser?.Username);
    }

    [Fact]
    public async Task Patch_ReturnsError_WhenEntityNotFound()
    {
        var result = await _userService.Patch(
            CreateUserPatch(u => u.Username, "NewUsername"),
            Guid.NewGuid()
        );
        Assert.True(result.HasErrorOfType<ResourceNotFoundException>());
    }

    [Fact]
    public async Task Patch_ReturnsError_WhenInvalidRegex()
    {
        var user = await _userRepository.CreateAndSaveAsync(GetTestUser());
        var patch = CreateUserPatch(u => u.Username, "to");
        var result = await _userService.Patch(patch, user.Id);
        var updatedUser = await _userRepository.GetByIdAsync(user.Id);
        Assert.True(result.HasErrorOfType<EntityValidationException>());
        Assert.NotEqual("", updatedUser?.Username);
    }
    
    [Fact]
    public async Task Patch_ReturnsError_WhenUniqueConstraint()
    {
        var user = await _userRepository.CreateAndSaveAsync(GetTestUser());
        var user2 = await _userRepository.CreateAndSaveAsync(GetTestUser());
        var patch = CreateUserPatch(u => u.Username, user2.Username);
        var result = await _userService.Patch(patch, user.Id);
        var updatedUser = await _userRepository.GetByIdAsync(user.Id);
        Assert.True(result.HasError);
        Assert.NotEqual(user2.Username, updatedUser?.Username);
    }
    
    [Fact]
    public async Task Patch_ReturnsError_WhenPatchingReadOnlyField()
    {
        var user = await _userRepository.CreateAndSaveAsync(GetTestUser());
        var patch = CreateUserPatch(u => u.Password, "testValue");
        var result = await _userService.Patch(patch, user.Id);
        Assert.True(result.HasError);
    }

    [Fact]
    public async Task Create_ReturnsSuccess_WhenEntityIsValid()
    {
        // var user = await _userRepository.CreateAndSaveAsync(GetTestUser());
        // var createdUser = await _userRepository.GetByIdAsync(user.Id);
        // Assert.False(result.HasError());
        // Assert.NotNull(createdUser);
        // Assert.Equal("NewUser", createdUser.Username);
    }

    [Fact]
    public async Task Delete_ReturnsSuccess_WhenEntityExists()
    {
        var user = await _userRepository.CreateAndSaveAsync(GetTestUser());
        var result = await _userService.Delete(user.Id);
        var deletedUser = await _userRepository.GetByIdAsync(user.Id);
        Assert.False(result.HasError);
        Assert.Null(deletedUser);
    }

    [Fact]
    public async Task Delete_ReturnsError_WhenEntityDoesNotExist()
    {
        var result = await _userService.Delete(Guid.NewGuid());
        Assert.True(result.HasError);
    }

    public void Dispose() => _connection.Dispose();
}