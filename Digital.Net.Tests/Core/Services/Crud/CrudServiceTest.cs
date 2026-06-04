using System.Linq.Expressions;
using System.Text.Json;
using Digital.Net.Core.Entities.Exceptions;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Core.Entities.Pivots;
using Digital.Net.Core.Http.Services.Crud;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Lib.Random;
using Digital.Net.Tests.Core.Factories;
using Microsoft.AspNetCore.JsonPatch;
using TUnit.Core.Interfaces;

namespace Digital.Net.Tests.Core.Services.Crud;

public class CrudServiceTest : UnitTest, IAsyncInitializer
{
    [ClassDataSource<DatabaseFixture>]
    public required DatabaseFixture DbFixture { get; init; }
    
    private static CrudTestEntity GetTestEntity() => new()
    {
        Name = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 8),
        UniqueField = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 16),
    };

    private static CrudTestChild GetTestChild() => new()
    {
        Label = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 8),
    };

    private static JsonElement ToElement(JsonPatchDocument<CrudTestEntity> patch) =>
        JsonSerializer.SerializeToElement(patch.Operations.Select(op => new
        {
            op = op.op,
            path = op.path,
            value = op.value,
        }));

    private static JsonElement CreatePatch<T>(Expression<Func<CrudTestEntity, T>> path, T value)
    {
        var patch = new JsonPatchDocument<CrudTestEntity>();
        patch.Replace(path, value);
        return ToElement(patch);
    }

    private CrudTestContext _context = null!;
    private CrudService<CrudTestContext, CrudTestEntity> _service = null!;

    public async Task InitializeAsync()
    {
        await DbFixture.EnsureCreatedAsync<CrudTestContext>();
        _context = DbFixture.CreateContext<CrudTestContext>();
        _service = new CrudService<CrudTestContext, CrudTestEntity>(
            _context,
            new PatchDispatcher<CrudTestEntity>([])
        );
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

        // Re-read from the database to assert the row is unchanged.
        await _context.Entry(entity1).ReloadAsync();

        await Assert.That(result.HasError).IsTrue();
        await Assert.That(entity1.UniqueField).IsNotEqualTo(entity2.UniqueField);
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

        var result = await _service.Patch(ToElement(patch), entity.Id);
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

        var result = await _service.Patch(ToElement(patch), entity.Id);
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

        var result = await _service.Patch(ToElement(patch), entity.Id);
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
    public async Task Patch_Succeeds_WhenPatchingUniqueFieldToSameValue()
    {
        var entity = GetTestEntity();
        await _context.TestEntities.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Patch UniqueField to the value it already has — should not be a conflict
        var result = await _service.Patch(CreatePatch(e => e.UniqueField, entity.UniqueField), entity.Id);
        await Assert.That(result.HasError).IsFalse();
    }
}
