using System.Linq.Expressions;
using Digital.Net.Core.Exceptions.types;
using Digital.Net.Core.Random;
using Digital.Net.Entities.Context;
using Digital.Net.Entities.Crud;
using Digital.Net.Entities.Exceptions;
using Digital.Net.Entities.Models.Pages;
using Digital.Net.Entities.Models.Users;
using Digital.Net.Tests.Core;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Data.Sqlite;

namespace Digital.Net.Entities.Test.Services;

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
    private readonly DigitalContext _context;
    private readonly ICrudService<User> _userService;
    private readonly ICrudService<Page> _pageService;
    private readonly CrudValidationService _crudValidationService;

    public CrudServiceTest()
    {
        _connection = SqliteInMemoryHelper.GetConnection();
        _context = _connection.CreateContext<DigitalContext>();
        _crudValidationService = new CrudValidationService(_context);
        _userService = new CrudService<User>(_context, _crudValidationService);
        _pageService = new CrudService<Page>(_context, _crudValidationService);
    }

    [Test]
    public async Task GetSchema_ReturnsCorrectSchema_WhenEntityHasProperties() =>
        await Assert.That(_crudValidationService.GetSchema<User>()[0].Name).IsEqualTo("Username");

    [Test]
    public async Task Patch_UpdatesEntity_WhenQueryIsValid()
    {
        var user = GetTestUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        
        var patch = CreateUserPatch(u => u.Username, "NewUsername");

        var result = await _userService.Patch(patch, user.Id);
        var updatedUser = await _context.Users.FindAsync(user.Id);
        await Assert.That(result).IsNotNull();
        await Assert.That(updatedUser?.Username).IsEqualTo("NewUsername");
    }

    [Test]
    public async Task Patch_UpdatesNestedEntity_WhenAddPatchIsValid()
    {
        var page = GetTestPage();
        await _context.Pages.AddAsync(page);
        await _context.SaveChangesAsync();
        
        var result =
            await _pageService.Patch(
                new JsonPatchDocument<Page>().Add(p => p.Metas,
                    new PageOpenGraph { Property = "TestMetaKey", Content = "TestMetaValue" }),
                page.Id);
        var updatedPage = await _context.Pages.FindAsync(page.Id);
        
        await _context.Entry(updatedPage!).Collection(p => p.Metas).LoadAsync();
        
        await Assert.That(result.HasError).IsFalse();
        await Assert.That(updatedPage!.Metas).IsNotEmpty();
    }

    [Test]
    public async Task Patch_UpdatesNestedEntity_WhenDeletePatchIsValid()
    {
        var page = GetTestPage();
        await _context.Pages.AddAsync(page);
        await _context.SaveChangesAsync();
        
        await _pageService.Patch(
            new JsonPatchDocument<Page>()
                .Add(p => p.Metas,
                    new PageOpenGraph { Property = "TestMetaKeyToRemove", Content = "TestMetaValue"  })
                .Add(p => p.Metas,
                    new PageOpenGraph { Property = "TestMetaKey", Content = "TestMetaValue" }),
            page.Id
        );
        await _pageService.Patch(new JsonPatchDocument<Page>().Remove(p => p.Metas[0]), page.Id);
        var updatedPage = await _context.Pages.FindAsync(page.Id);
        await Assert.That(updatedPage!.Metas.Any(m => m.Property == "TestMetaKeyToRemove")).IsFalse();
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
        var user = GetTestUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        
        var patch = CreateUserPatch(u => u.Username, "to");
        var result = await _userService.Patch(patch, user.Id);
        var updatedUser = await _context.Users.FindAsync(user.Id);
        
        await Assert.That(result.HasErrorOfType<EntityValidationException>()).IsTrue();
        await Assert.That(updatedUser?.Username).IsNotEqualTo("");
    }

    [Test]
    public async Task Patch_ReturnsError_WhenUniqueConstraint()
    {
        var user = GetTestUser();
        var user2 = GetTestUser();
        await _context.Users.AddAsync(user);
        await _context.Users.AddAsync(user2);
        await _context.SaveChangesAsync();
        
        var patch = CreateUserPatch(u => u.Username, user2.Username);
        var result = await _userService.Patch(patch, user.Id);
        var updatedUser = await _context.Users.FindAsync(user.Id);
        
        await Assert.That(result.HasError).IsTrue();
        await Assert.That(updatedUser?.Username).IsNotEqualTo(user2.Username);
    }

    [Test]
    public async Task Patch_ReturnsError_WhenPatchingReadOnlyField()
    {
        var user = GetTestUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

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
        var user = GetTestUser();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _userService.Delete(user.Id);
        var deletedUser = await _context.Users.FindAsync(user.Id);
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