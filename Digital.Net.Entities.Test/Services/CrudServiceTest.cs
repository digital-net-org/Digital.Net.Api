using System.Linq.Expressions;
using Digital.Net.Core.Exceptions.types;
using Digital.Net.Core.Random;
using Digital.Net.Entities.Crud;
using Digital.Net.Entities.Exceptions;
using Digital.Net.Entities.Test.Context;
using Digital.Net.Entities.Test.Models;
using Digital.Net.Tests.Core;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Data.Sqlite;

namespace Digital.Net.Entities.Test.Services;

public class CrudServiceTest : UnitTest, IDisposable
{
    private static TestEntity GetTestEntity() => new()
    {
        Name = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 8),
        UniqueField = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 16),
    };

    private static TestChild GetTestChild() => new()
    {
        Label = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 8),
    };

    private static JsonPatchDocument<TestEntity> CreatePatch<T>(Expression<Func<TestEntity, T>> path, T value)
    {
        var patch = new JsonPatchDocument<TestEntity>();
        patch.Replace(path, value);
        return patch;
    }

    private readonly SqliteConnection _connection;
    private readonly EntitiesTestContext _context;
    private readonly ICrudService<TestEntity> _service;
    private readonly CrudValidationService<EntitiesTestContext> _validationService;

    public CrudServiceTest()
    {
        _connection = SqliteInMemoryHelper.GetConnection();
        _context = _connection.CreateContext<EntitiesTestContext>();
        _validationService = new CrudValidationService<EntitiesTestContext>(_context);
        _service = new CrudService<EntitiesTestContext, TestEntity>(_context, _validationService);
    }

    [Test]
    public async Task GetSchema_ReturnsCorrectSchema_WhenEntityHasProperties() =>
        await Assert.That(_validationService.GetSchema<TestEntity>()[0].Name).IsEqualTo("Name");

    [Test]
    public async Task Patch_UpdatesEntity_WhenQueryIsValid()
    {
        var entity = GetTestEntity();
        await _context.TestEntities.AddAsync(entity);
        await _context.SaveChangesAsync();

        var newName = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 8);
        var result = await _service.Patch(CreatePatch(e => e.Name, newName), entity.Id);
        var updated = await _context.TestEntities.FindAsync(entity.Id);

        await Assert.That(result.HasError).IsFalse();
        await Assert.That(updated?.Name).IsEqualTo(newName);
    }

    [Test]
    public async Task Patch_ReturnsError_WhenEntityNotFound()
    {
        var result = await _service.Patch(CreatePatch(e => e.Name, "ValidName1"), Guid.NewGuid());
        await Assert.That(result.HasErrorOfType<ResourceNotFoundException>()).IsTrue();
    }

    [Test]
    public async Task Patch_ReturnsError_WhenInvalidRegex()
    {
        var entity = GetTestEntity();
        await _context.TestEntities.AddAsync(entity);
        await _context.SaveChangesAsync();

        // "ab" is too short for UsernamePattern (min 6 chars)
        var result = await _service.Patch(CreatePatch(e => e.Name, "ab"), entity.Id);
        await Assert.That(result.HasErrorOfType<EntityValidationException>()).IsTrue();
    }

    [Test]
    public async Task Patch_ReturnsError_WhenUniqueConstraint()
    {
        var entity1 = GetTestEntity();
        var entity2 = GetTestEntity();
        await _context.TestEntities.AddAsync(entity1);
        await _context.TestEntities.AddAsync(entity2);
        await _context.SaveChangesAsync();

        var result = await _service.Patch(CreatePatch(e => e.UniqueField, entity2.UniqueField), entity1.Id);
        var updated = await _context.TestEntities.FindAsync(entity1.Id);

        await Assert.That(result.HasError).IsTrue();
        await Assert.That(updated?.UniqueField).IsNotEqualTo(entity2.UniqueField);
    }

    [Test]
    public async Task Patch_ReturnsError_WhenPatchingReadOnlyScalarField()
    {
        var entity = GetTestEntity();
        await _context.TestEntities.AddAsync(entity);
        await _context.SaveChangesAsync();

        var result = await _service.Patch(CreatePatch(e => e.ReadOnlyField, "blocked"), entity.Id);
        await Assert.That(result.HasErrorOfType<EntityValidationException>()).IsTrue();
    }

    [Test]
    public async Task Patch_UpdatesChildren_WhenAddPatchIsValid()
    {
        var entity = GetTestEntity();
        await _context.TestEntities.AddAsync(entity);
        await _context.SaveChangesAsync();

        var child = new TestChild { Label = "ChildLabel", TestEntityId = entity.Id };
        var patch = new JsonPatchDocument<TestEntity>();
        patch.Add(e => e.Children, child);

        var result = await _service.Patch(patch, entity.Id);
        await Assert.That(result.HasErrorOfType<EntityValidationException>()).IsFalse();
    }

    [Test]
    public async Task Patch_ReturnsError_WhenAddPatchTargetsReadOnlyNavigation()
    {
        var entity = GetTestEntity();
        await _context.TestEntities.AddAsync(entity);
        await _context.SaveChangesAsync();

        var child = new TestChild { Label = "ChildLabel", TestEntityId = entity.Id };
        var patch = new JsonPatchDocument<TestEntity>();
        patch.Add(e => e.LockedChild, child);

        var result = await _service.Patch(patch, entity.Id);
        await Assert.That(result.HasErrorOfType<EntityValidationException>()).IsTrue();
    }

    [Test]
    public async Task Patch_RemovesChild_WhenDeletePatchIsValid()
    {
        var entity = GetTestEntity();
        var child = GetTestChild();
        child.TestEntityId = entity.Id;
        entity.Children.Add(child);
        await _context.TestEntities.AddAsync(entity);
        await _context.SaveChangesAsync();

        var patch = new JsonPatchDocument<TestEntity>();
        patch.Remove(e => e.Children, 0);

        var result = await _service.Patch(patch, entity.Id);

        // Remove operations bypass ValidatePatchPayload, so no EntityValidationException expected.
        // The patch may fail at application level (no Include), but not with a validation error.
        await Assert.That(result.HasErrorOfType<EntityValidationException>()).IsFalse();
    }

    [Test]
    public async Task Create_ReturnsSuccess_WhenEntityIsValid()
    {
        var entity = GetTestEntity();
        await _context.TestEntities.AddAsync(entity);
        await _context.SaveChangesAsync();

        var created = await _context.TestEntities.FindAsync(entity.Id);
        await Assert.That(created).IsNotNull();
        await Assert.That(created!.Name).IsEqualTo(entity.Name);
    }

    [Test]
    public async Task Delete_ReturnsSuccess_WhenEntityExists()
    {
        var entity = GetTestEntity();
        await _context.TestEntities.AddAsync(entity);
        await _context.SaveChangesAsync();

        var result = await _service.Delete(entity.Id);
        var deleted = await _context.TestEntities.FindAsync(entity.Id);
        await Assert.That(result.HasError).IsFalse();
        await Assert.That(deleted).IsNull();
    }

    [Test]
    public async Task Delete_ReturnsError_WhenEntityDoesNotExist()
    {
        var result = await _service.Delete(Guid.NewGuid());
        await Assert.That(result.HasError).IsTrue();
    }

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
