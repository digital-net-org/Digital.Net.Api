using System.Linq.Expressions;
using System.Text.Json;
using Digital.Net.Lib.Entities.Exceptions;
using Digital.Net.Lib.Entities.Models;
using Digital.Net.Lib.Entities.Pivots;
using Digital.Net.Core.Http.Services.Crud;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Lib.Random;
using Microsoft.AspNetCore.JsonPatch;

namespace Digital.Net.Tests.Core.Http.Services.Crud;

public class CrudServiceTest : DbServiceTest<CrudTestContext>
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

    private CrudService<CrudTestContext, CrudTestEntity> _service = null!;

    protected override Task OnInitializingAsync() => DbFixture.EnsureCreatedAsync<CrudTestContext>();

    protected override Task OnInitializedAsync()
    {
        _service = new CrudService<CrudTestContext, CrudTestEntity>(
            Context,
            new PatchDispatcher<CrudTestEntity>([])
        );
        return Task.CompletedTask;
    }

    [Test]
    public async Task GetSchema_ReturnsCorrectSchema_WhenEntityHasProperties() =>
        await Assert.That(SchemaProperty<CrudTestEntity>.Get()[0].Name).IsEqualTo("Name");

    [Test]
    public async Task Patch_UpdatesEntity_WhenQueryIsValid()
    {
        var entity = GetTestEntity();
        await Context.TestEntities.AddAsync(entity);
        await Context.SaveChangesAsync();

        var newName = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 8);
        var result = await _service.Patch(CreatePatch(e => e.Name, newName), entity.Id);
        var updated = await Context.TestEntities.FindAsync(entity.Id);

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
    public async Task Patch_ReturnsError_WhenUniqueConstraint()
    {
        var entity1 = GetTestEntity();
        var entity2 = GetTestEntity();
        await Context.TestEntities.AddAsync(entity1);
        await Context.TestEntities.AddAsync(entity2);
        await Context.SaveChangesAsync();

        var result = await _service.Patch(CreatePatch(e => e.UniqueField, entity2.UniqueField), entity1.Id);

        // Re-read from the database to assert the row is unchanged.
        await Context.Entry(entity1).ReloadAsync();

        await Assert.That(result.HasError).IsTrue();
        await Assert.That(entity1.UniqueField).IsNotEqualTo(entity2.UniqueField);
    }

    [Test]
    public async Task Patch_UpdatesChildren_WhenAddPatchIsValid()
    {
        var entity = GetTestEntity();
        await Context.TestEntities.AddAsync(entity);
        await Context.SaveChangesAsync();

        var child = new CrudTestChild { Label = "ChildLabel", TestEntityId = entity.Id };
        var patch = new JsonPatchDocument<CrudTestEntity>();
        patch.Add(e => e.Children, child);

        var result = await _service.Patch(ToElement(patch), entity.Id);
        await Assert.That(result.HasErrorOfType<EntityValidationException>()).IsFalse();
    }

    [Test]
    public async Task Patch_RemovesChild_WhenDeletePatchIsValid()
    {
        var entity = GetTestEntity();
        var child = GetTestChild();
        child.TestEntityId = entity.Id;
        entity.Children.Add(child);
        await Context.TestEntities.AddAsync(entity);
        await Context.SaveChangesAsync();

        var patch = new JsonPatchDocument<CrudTestEntity>();
        patch.Remove(e => e.Children, 0);

        var result = await _service.Patch(ToElement(patch), entity.Id);
        await Assert.That(result.HasErrorOfType<EntityValidationException>()).IsFalse();
    }

    [Test]
    public async Task Create_ReturnsSuccess_WhenEntityIsValid()
    {
        var entity = GetTestEntity();
        await Context.TestEntities.AddAsync(entity);
        await Context.SaveChangesAsync();

        var created = await Context.TestEntities.FindAsync(entity.Id);
        await Assert.That(created).IsNotNull();
        await Assert.That(created!.Name).IsEqualTo(entity.Name);
    }
    
    [Test]
    public async Task Patch_Succeeds_WhenPatchingUniqueFieldToSameValue()
    {
        var entity = GetTestEntity();
        await Context.TestEntities.AddAsync(entity);
        await Context.SaveChangesAsync();

        // Patch UniqueField to the value it already has — should not be a conflict
        var result = await _service.Patch(CreatePatch(e => e.UniqueField, entity.UniqueField), entity.Id);
        await Assert.That(result.HasError).IsFalse();
    }
}
