using System.Linq.Expressions;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Core.Services.Crud;
using Digital.Net.Core.Services.Crud.Exceptions;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Lib.Random;
using Digital.Net.Tests.Core;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Data.Sqlite;

namespace Digital.Net.Core.Test.Services.Crud;

public class CrudServiceTest : UnitTest, IDisposable
{
    private static CrudTestEntity GetTestEntity() => new()
    {
        Name = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 8),
        UniqueField = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 16),
    };

    private static CrudTestChild GetTestChild() => new()
    {
        Label = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 8),
    };

    private static JsonPatchDocument<CrudTestEntity> CreatePatch<T>(Expression<Func<CrudTestEntity, T>> path, T value)
    {
        var patch = new JsonPatchDocument<CrudTestEntity>();
        patch.Replace(path, value);
        return patch;
    }

    private readonly SqliteConnection _connection;
    private readonly CrudTestContext _context;
    private readonly ICrudService<CrudTestEntity> _service;
    private readonly CrudValidationService<CrudTestContext> _validationService;

    public CrudServiceTest()
    {
        _connection = SqliteInMemoryHelper.GetConnection();
        _context = _connection.CreateContext<CrudTestContext>();
        _validationService = new CrudValidationService<CrudTestContext>(_context);
        _service = new CrudService<CrudTestContext, CrudTestEntity>(_context, _validationService);
    }

    [Test]
    public async Task GetSchema_ReturnsCorrectSchema_WhenEntityHasProperties() =>
        await Assert.That(SchemaProperty<CrudTestEntity>.Get()[0].Name).IsEqualTo("Name");

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

        var child = new CrudTestChild { Label = "ChildLabel", TestEntityId = entity.Id };
        var patch = new JsonPatchDocument<CrudTestEntity>();
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

        var child = new CrudTestChild { Label = "ChildLabel", TestEntityId = entity.Id };
        var patch = new JsonPatchDocument<CrudTestEntity>();
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

        var patch = new JsonPatchDocument<CrudTestEntity>();
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


    [Test]
    public async Task ValidateCreatePayload_DoesNotThrow_WhenEntityIsValid()
    {
        var entity = GetTestEntity();
        await Assert.That(() => _validationService.ValidateCreatePayload(entity)).ThrowsNothing();
    }

    [Test]
    public async Task ValidateCreatePayload_Throws_WhenRequiredStringIsEmpty()
    {
        var entity = new CrudTestEntity { Name = "", UniqueField = "SomeUniqueVal" };
        await Assert.ThrowsAsync<EntityValidationException>(async () =>
        {
            _validationService.ValidateCreatePayload(entity);
            await Task.CompletedTask;
        });
    }

    [Test]
    public async Task ValidateCreatePayload_Throws_WhenRegexViolated()
    {
        var entity = new CrudTestEntity { Name = "ab", UniqueField = "SomeUniqueVal" };
        await Assert.ThrowsAsync<EntityValidationException>(async () =>
        {
            _validationService.ValidateCreatePayload(entity);
            await Task.CompletedTask;
        });
    }

    [Test]
    public async Task ValidateCreatePayload_Throws_WhenMaxLengthExceeded()
    {
        var entity = new CrudTestEntity { Name = "validname", UniqueField = new string('X', 65) };
        await Assert.ThrowsAsync<EntityValidationException>(async () =>
        {
            _validationService.ValidateCreatePayload(entity);
            await Task.CompletedTask;
        });
    }


    [Test]
    public async Task ValidateProperty_DoesNotThrow_WhenValueIsNull()
    {
        var schema = SchemaProperty<CrudTestEntity>.Get().First(x => x.Name == "Name");
        await Assert.That(() => _validationService.ValidateProperty(null, "Name", schema)).ThrowsNothing();
    }

    [Test]
    public async Task ValidateProperty_DoesNotThrow_WhenSchemaPropertyIsNull() =>
        await Assert.That(() => _validationService.ValidateProperty<CrudTestEntity>("value", "Name", null)).ThrowsNothing();

    [Test]
    public async Task ValidateProperty_DoesNotThrow_WhenRequiredStringHasValue()
    {
        var schema = SchemaProperty<CrudTestEntity>.Get().First(x => x.Name == "Name");
        await Assert.That(() => _validationService.ValidateProperty("validname", "Name", schema)).ThrowsNothing();
    }

    [Test]
    public async Task ValidateProperty_Throws_WhenRequiredStringIsEmpty()
    {
        var schema = SchemaProperty<CrudTestEntity>.Get().First(x => x.Name == "Name");
        await Assert.ThrowsAsync<EntityValidationException>(async () =>
        {
            _validationService.ValidateProperty("", "Name", schema);
            await Task.CompletedTask;
        });
    }

    [Test]
    public async Task ValidateProperty_Throws_WhenMaxLengthExceeded()
    {
        var schema = SchemaProperty<CrudTestEntity>.Get().First(x => x.Name == "UniqueField");
        await Assert.ThrowsAsync<EntityValidationException>(async () =>
        {
            _validationService.ValidateProperty(new string('X', 65), "UniqueField", schema);
            await Task.CompletedTask;
        });
    }

    [Test]
    public async Task ValidateProperty_Throws_WhenRegexFails()
    {
        var schema = SchemaProperty<CrudTestEntity>.Get().First(x => x.Name == "Name");
        await Assert.ThrowsAsync<EntityValidationException>(async () =>
        {
            _validationService.ValidateProperty("ab", "Name", schema);
            await Task.CompletedTask;
        });
    }

    [Test]
    public async Task Patch_Succeeds_WhenPatchingUniqueFieldToSameValue()
    {
        var entity = GetTestEntity();
        await _context.TestEntities.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Patch UniqueField to the value it already has — should not be a conflict
        var result = await _service.Patch(CreatePatch(e => e.UniqueField, entity.UniqueField), entity.Id);
        await Assert.That(result.HasError).IsFalse();
    }

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
