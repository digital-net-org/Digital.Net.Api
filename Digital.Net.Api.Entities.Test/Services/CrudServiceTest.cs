using System.Linq.Expressions;
using Digital.Net.Api.Core.Exceptions;
using Digital.Net.Api.Core.Random;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Crud;
using Digital.Net.Api.Entities.Exceptions;
using Digital.Net.Api.Entities.Models.Pages;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Tests.Core;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Data.Sqlite;

namespace Digital.Net.Api.Entities.Test.Services;

public class CrudServiceTest : UnitTest, IDisposable
{
    private static User GetTestUser() => new()
    {
            Username = Randomizer.GenerateRandomString(),
            Login = Randomizer.GenerateRandomString(),
            Password = Randomizer.GenerateRandomString(),
            Email = Randomizer.GenerateRandomEmail()
    };

    private static Page GetTestPage() => new()
    {
        Title = Randomizer.GenerateRandomString(),
        Description = Randomizer.GenerateRandomString(),
        Path = Randomizer.GenerateRandomString(),
        IsIndexed = false,
        IsPublished = false,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    private static JsonPatchDocument<User> CreateUserPatch<T>(Expression<Func<User, T>> path, T value)
    {
        var patch = new JsonPatchDocument<User>();
        patch.Replace(path, value);
        return patch;
    }

    private readonly SqliteConnection _connection;
    private readonly Repository<User, DigitalContext> _userRepository;
    private readonly Repository<Page, DigitalContext> _pageRepository;
    private readonly ICrudService<User, DigitalContext> _userService;
    private readonly ICrudService<Page, DigitalContext> _pageService;
    private readonly CrudValidationService<DigitalContext> _crudValidationService;

    public CrudServiceTest()
    {
        _connection = SqliteInMemoryHelper.GetConnection();
        var context = _connection.CreateContext<DigitalContext>();
        _crudValidationService = new CrudValidationService<DigitalContext>(context);
        _userRepository = new Repository<User, DigitalContext>(context);
        _pageRepository = new Repository<Page, DigitalContext>(context);
        _userService = new CrudService<User, DigitalContext>(_userRepository, _crudValidationService);
        _pageService = new CrudService<Page, DigitalContext>(_pageRepository, _crudValidationService);
    }

    [Test]
    public async Task GetSchema_ReturnsCorrectSchema_WhenEntityHasProperties() =>
        await Assert.That(_crudValidationService.GetSchema<User>()[0].Name).IsEqualTo("Username");

    [Test]
    public async Task Patch_UpdatesEntity_WhenQueryIsValid()
    {
        var user = await _userRepository.CreateAndSaveAsync(GetTestUser());
        var patch = CreateUserPatch(u => u.Username, "NewUsername");

        var result = await _userService.Patch(patch, user.Id);
        var updatedUser = await _userRepository.GetByIdAsync(user.Id);
        await Assert.That(result).IsNotNull();
        await Assert.That(updatedUser?.Username).IsEqualTo("NewUsername");
    }

    [Test]
    public async Task Patch_UpdatesNestedEntity_WhenAddPatchIsValid()
    {
        var page = await _pageRepository.CreateAndSaveAsync(GetTestPage());
        var result =
            await _pageService.Patch(
                new JsonPatchDocument<Page>().Add(p => p.Metas,
                    new PageMeta { Key = "TestMetaKey", Value = "TestMetaValue", Content = "TestContent" }), page.Id);
        var updatedPage = await _pageRepository.GetByIdAsync(page.Id);
        await Assert.That(result.HasError).IsFalse();
        await Assert.That(updatedPage!.Metas).IsNotEmpty();
    }

    [Test]
    public async Task Patch_UpdatesNestedEntity_WhenDeletePatchIsValid()
    {
        var page = await _pageRepository.CreateAndSaveAsync(GetTestPage());
        await _pageService.Patch(
            new JsonPatchDocument<Page>()
                .Add(p => p.Metas,
                    new PageMeta { Key = "TestMetaKeyToRemove", Value = "TestMetaValue", Content = "TestContent" })
                .Add(p => p.Metas,
                    new PageMeta { Key = "TestMetaKey", Value = "TestMetaValue", Content = "TestContent" }),
            page.Id
        );
        await _pageService.Patch(new JsonPatchDocument<Page>().Remove(p => p.Metas[0]), page.Id);
        var updatedPage = await _pageRepository.GetByIdAsync(page.Id);
        await Assert.That(updatedPage!.Metas.Any(m => m.Key == "TestMetaKeyToRemove")).IsFalse();
    }

    [Test]
    public async Task Patch_ReturnsError_WhenEntityNotFound()
    {
        var result = await _userService.Patch(
            CreateUserPatch(u => u.Username, "NewUsername"),
            Guid.NewGuid()
        );
        await Assert.That(result.HasErrorOfType<ResourceNotFoundException>()).IsTrue();
    }

    [Test]
    public async Task Patch_ReturnsError_WhenInvalidRegex()
    {
        var user = await _userRepository.CreateAndSaveAsync(GetTestUser());
        var patch = CreateUserPatch(u => u.Username, "to");
        var result = await _userService.Patch(patch, user.Id);
        var updatedUser = await _userRepository.GetByIdAsync(user.Id);
        await Assert.That(result.HasErrorOfType<EntityValidationException>()).IsTrue();
        await Assert.That(updatedUser?.Username).IsNotEqualTo("");
    }

    [Test]
    public async Task Patch_ReturnsError_WhenUniqueConstraint()
    {
        var user = await _userRepository.CreateAndSaveAsync(GetTestUser());
        var user2 = await _userRepository.CreateAndSaveAsync(GetTestUser());
        var patch = CreateUserPatch(u => u.Username, user2.Username);
        var result = await _userService.Patch(patch, user.Id);
        var updatedUser = await _userRepository.GetByIdAsync(user.Id);
        await Assert.That(result.HasError).IsTrue();
        await Assert.That(updatedUser?.Username).IsNotEqualTo(user2.Username);
    }

    [Test]
    public async Task Patch_ReturnsError_WhenPatchingReadOnlyField()
    {
        var user = await _userRepository.CreateAndSaveAsync(GetTestUser());
        var patch = CreateUserPatch(u => u.Password, "testValue");
        var result = await _userService.Patch(patch, user.Id);
        await Assert.That(result.HasError).IsTrue();
    }

    [Test]
    public async Task Create_ReturnsSuccess_WhenEntityIsValid()
    {
        // var user = await _userRepository.CreateAndSaveAsync(GetTestUser());
        // var createdUser = await _userRepository.GetByIdAsync(user.Id);
        // await Assert.That(result.HasError()).IsFalse();
        // await Assert.That(createdUser).IsNotNull();
        // await Assert.That(createdUser.Username).IsEqualTo("NewUser");
    }

    [Test]
    public async Task Delete_ReturnsSuccess_WhenEntityExists()
    {
        var user = await _userRepository.CreateAndSaveAsync(GetTestUser());
        var result = await _userService.Delete(user.Id);
        var deletedUser = await _userRepository.GetByIdAsync(user.Id);
        await Assert.That(result.HasError).IsFalse();
        await Assert.That(deletedUser).IsNull();
    }

    [Test]
    public async Task Delete_ReturnsError_WhenEntityDoesNotExist()
    {
        var result = await _userService.Delete(Guid.NewGuid());
        await Assert.That(result.HasError).IsTrue();
    }

    public void Dispose() => _connection.Dispose();
}