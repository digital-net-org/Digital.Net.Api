using Digital.Net.Core.Entities.Exceptions;
using Digital.Net.Core.Services.Crud;
using Digital.Net.Lib.Random;
using Digital.Net.Tests.Core;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Digital.Net.Core.Test.Services.Crud;

public class CrudValidatorTest : UnitTest
{
    private static CrudTestEntity GetValidEntity() => new()
    {
        Name = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 8),
        UniqueField = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 16),
    };

    private static CrudTestChild GetValidChild() => new()
    {
        Label = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 8),
    };

    [Test]
    public async Task ValidateCreatePayload_DoesNotThrow_WhenEntityIsValid()
    {
        var entity = GetValidEntity();
        await Assert.That(() => CrudValidator.ValidateCreatePayload(entity)).ThrowsNothing();
    }

    [Test]
    public async Task ValidateCreatePayload_DoesNotThrow_WhenIdentityIsDefaultGuid()
    {
        var entity = GetValidEntity();
        await Assert.That(entity.Id).IsEqualTo(Guid.Empty);
        await Assert.That(() => CrudValidator.ValidateCreatePayload(entity)).ThrowsNothing();
    }

    [Test]
    public async Task ValidateCreatePayload_Throws_WhenRequiredStringIsEmpty()
    {
        var entity = GetValidEntity();
        entity.UniqueField = "";
        await Assert.ThrowsAsync<EntityValidationException>(async () =>
        {
            CrudValidator.ValidateCreatePayload(entity);
            await Task.CompletedTask;
        });
    }

    [Test]
    public async Task ValidateCreatePayload_Throws_WhenRegexFails()
    {
        var entity = GetValidEntity();
        entity.Name = "ab";
        await Assert.ThrowsAsync<EntityValidationException>(async () =>
        {
            CrudValidator.ValidateCreatePayload(entity);
            await Task.CompletedTask;
        });
    }

    [Test]
    public async Task ValidateCreatePayload_Throws_WhenOneOfValueIsUnknown()
    {
        var entity = GetValidEntity();
        entity.OneOfField = "delta";
        await Assert.ThrowsAsync<EntityValidationException>(async () =>
        {
            CrudValidator.ValidateCreatePayload(entity);
            await Task.CompletedTask;
        });
    }

    [Test]
    public async Task ValidatePatchPayload_DoesNotThrow_WhenValueIsNull()
    {
        var patch = new JsonPatchDocument<CrudTestEntity>();
        patch.Replace(e => e.OneOfField, null);
        await Assert.That(() => CrudValidator.ValidatePatchPayload(patch)).ThrowsNothing();
    }

    [Test]
    public async Task ValidatePatchPayload_DoesNotThrow_WhenOpIsNotAddOrReplace()
    {
        var patch = new JsonPatchDocument<CrudTestEntity>();
        patch.Operations.Add(new Operation<CrudTestEntity>
        {
            op = "test", path = "/Name", value = "ab",
        });
        await Assert.That(() => CrudValidator.ValidatePatchPayload(patch)).ThrowsNothing();
    }

    [Test]
    public async Task ValidatePatchPayload_DoesNotThrow_WhenPathIsDeeperThanTwoLevels()
    {
        var patch = new JsonPatchDocument<CrudTestEntity>();
        patch.Operations.Add(new Operation<CrudTestEntity>
        {
            op = "replace", path = "/Children/0/Label", value = "ab",
        });
        await Assert.That(() => CrudValidator.ValidatePatchPayload(patch)).ThrowsNothing();
    }

    [Test]
    public async Task ValidatePatchPayload_DoesNotThrow_WhenPropertyIsNotInSchema()
    {
        var patch = new JsonPatchDocument<CrudTestEntity>();
        patch.Operations.Add(new Operation<CrudTestEntity>
        {
            op = "replace", path = "/UnknownField", value = "x",
        });
        await Assert.That(() => CrudValidator.ValidatePatchPayload(patch)).ThrowsNothing();
    }

    [Test]
    public async Task ValidatePatchPayload_DoesNotThrow_WhenScalarReplaceIsValid()
    {
        var patch = new JsonPatchDocument<CrudTestEntity>();
        patch.Replace(e => e.Name, Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 8));
        await Assert.That(() => CrudValidator.ValidatePatchPayload(patch)).ThrowsNothing();
    }

    [Test]
    public async Task ValidatePatchPayload_Throws_WhenScalarReplaceTargetsReadOnlyField()
    {
        var patch = new JsonPatchDocument<CrudTestEntity>();
        patch.Replace(e => e.ReadOnlyField, "blocked");
        await Assert.ThrowsAsync<EntityValidationException>(async () =>
        {
            CrudValidator.ValidatePatchPayload(patch);
            await Task.CompletedTask;
        });
    }

    [Test]
    public async Task ValidatePatchPayload_Throws_WhenScalarReplaceFailsOneOf()
    {
        var patch = new JsonPatchDocument<CrudTestEntity>();
        patch.Replace(e => e.OneOfField, "delta");
        await Assert.ThrowsAsync<EntityValidationException>(async () =>
        {
            CrudValidator.ValidatePatchPayload(patch);
            await Task.CompletedTask;
        });
    }

    [Test]
    public async Task ValidatePatchPayload_DoesNotThrow_WhenAddingValidChildToCollection()
    {
        var patch = new JsonPatchDocument<CrudTestEntity>();
        patch.Add(e => e.Children, GetValidChild());
        await Assert.That(() => CrudValidator.ValidatePatchPayload(patch)).ThrowsNothing();
    }

    [Test]
    public async Task ValidatePatchPayload_Throws_WhenAddingChildWithInvalidProperty()
    {
        var patch = new JsonPatchDocument<CrudTestEntity>();
        patch.Add(e => e.Children, new CrudTestChild { Label = "" });
        await Assert.ThrowsAsync<EntityValidationException>(async () =>
        {
            CrudValidator.ValidatePatchPayload(patch);
            await Task.CompletedTask;
        });
    }

    [Test]
    public async Task ValidatePatchPayload_Throws_WhenAddingChildToReadOnlyNavigation()
    {
        // Single-nav read-only path falls through to schemaProperty.ValidateMutation,
        // which rejects the mutation because LockedChild is [ReadOnly].
        var patch = new JsonPatchDocument<CrudTestEntity>();
        patch.Add(e => e.LockedChild, GetValidChild());
        await Assert.ThrowsAsync<EntityValidationException>(async () =>
        {
            CrudValidator.ValidatePatchPayload(patch);
            await Task.CompletedTask;
        });
    }

    [Test]
    public async Task ValidatePatchPayload_Throws_WhenDepthTwoTargetsReadOnlyParent()
    {
        // Hits the depth-2 branch where parentSchemaProperty.IsReadOnly is true.
        var patch = new JsonPatchDocument<CrudTestEntity>();
        patch.Operations.Add(new Operation<CrudTestEntity>
        {
            op = "add", path = "/LockedChild/-", value = GetValidChild(),
        });
        await Assert.ThrowsAsync<EntityValidationException>(async () =>
        {
            CrudValidator.ValidatePatchPayload(patch);
            await Task.CompletedTask;
        });
    }
}
