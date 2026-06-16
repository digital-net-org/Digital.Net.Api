using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.ApiKeys;
using Digital.Net.Core.Services.ApiKeys;
using Digital.Net.Core.Services.ApiKeys.Exceptions;
using Digital.Net.Tests.Core.Factories.Data;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Tests.Core.Services.ApiKeys;

public class ApiKeyServiceTest : DbServiceTest<DigitalContext>
{
    private ApiKeyService _service = null!;

    protected override Task OnInitializedAsync()
    {
        _service = new ApiKeyService(Context);
        return Task.CompletedTask;
    }

    [Test]
    public async Task CreateAsync_ReturnsPlaintextKey_AndStoresHash()
    {
        var user = Context.BuildTestUser();
        var result = await _service.CreateAsync(user.Id, $"Key-{TestId}", null);

        await Assert.That(result.HasError).IsFalse();
        await Assert.That(result.Value).IsNotNull();
        await Assert.That(result.Value!.Length).IsGreaterThan(0);

        var hashed = ApiKey.Hash(result.Value);
        var exists = await Context.ApiKeys.AnyAsync(k => k.Key == hashed && k.UserId == user.Id);
        await Assert.That(exists).IsTrue();
    }

    [Test]
    public async Task CreateAsync_ExpiresToDefaultExpiration()
    {
        var user = Context.BuildTestUser();
        var name = $"DefaultExp-{TestId}";

        var result = await _service.CreateAsync(user.Id, name, null);
        await Assert.That(result.HasError).IsFalse();

        var stored = await Context.ApiKeys.FirstAsync(k => k.UserId == user.Id && k.Name == name);
        var expected = DateTime.UtcNow.Add(ApiKeyService.DefaultExpiration);

        await Assert.That(stored.ExpiredAt).IsNotNull();
        await Assert.That((stored.ExpiredAt!.Value - expected).TotalMinutes).IsLessThan(1);
    }

    [Test]
    public async Task CreateAsync_ExpiresToProvidedExpiration()
    {
        var user = Context.BuildTestUser();
        var name = $"Custom-{TestId}";
        var customExpiry = DateTime.UtcNow.AddYears(1);

        var result = await _service.CreateAsync(user.Id, name, customExpiry);
        await Assert.That(result.HasError).IsFalse();

        var stored = await Context.ApiKeys.FirstAsync(k => k.UserId == user.Id && k.Name == name);
        await Assert.That((stored.ExpiredAt!.Value - customExpiry).TotalMinutes).IsLessThan(1);
    }

    [Test]
    public async Task CreateAsync_ReturnsError_WhenExpirationInPast()
    {
        var user = Context.BuildTestUser();
        var pastDate = DateTime.UtcNow.AddDays(-1);

        var result = await _service.CreateAsync(user.Id, $"Past-{TestId}", pastDate);

        await Assert.That(result.HasError).IsTrue();
        await Assert.That(result.HasErrorOfType<ExpiredAtInThePastException>()).IsTrue();
    }

    [Test]
    public async Task CreateAsync_ReturnsError_WhenMaxKeysReached()
    {
        var user = Context.BuildTestUser();
        for (var i = 0; i < ApiKeyService.MaxApiKeysPerUser; i++)
            await _service.CreateAsync(user.Id, $"Key-{i}-{TestId}", null);

        var result = await _service.CreateAsync(user.Id, $"OneTooMany-{TestId}", null);

        await Assert.That(result.HasErrorOfType<MaxApiKeysReachedException>()).IsTrue();
    }

    [Test]
    public async Task CreateAsync_ReturnsError_OnDuplicateNameForSameUser()
    {
        var user = Context.BuildTestUser();
        var name = $"Dup-{TestId}";

        await _service.CreateAsync(user.Id, name, null);
        var result = await _service.CreateAsync(user.Id, name, null);

        await Assert.That(result.HasErrorOfType<DuplicateApiKeyNameException>()).IsTrue();
    }

    [Test]
    public async Task CreateAsync_AllowsSameNameAcrossUsers()
    {
        var user1 = Context.BuildTestUser();
        var user2 = Context.BuildTestUser();
        var name = $"Shared-{TestId}";

        var r1 = await _service.CreateAsync(user1.Id, name, null);
        var r2 = await _service.CreateAsync(user2.Id, name, null);

        await Assert.That(r1.HasError).IsFalse();
        await Assert.That(r2.HasError).IsFalse();
    }

    [Test]
    public async Task CreateAsync_ReturnsError_OnMalformedName()
    {
        var user = Context.BuildTestUser();

        var result = await _service.CreateAsync(user.Id, "Invalid!@#$%", null);

        await Assert.That(result.HasErrorOfType<ApiKeyNameMalformedException>()).IsTrue();
    }

    [Test]
    public async Task CreateAsync_ReturnsError_OnEmptyName()
    {
        var user = Context.BuildTestUser();

        var result = await _service.CreateAsync(user.Id, string.Empty, null);

        await Assert.That(result.HasErrorOfType<ApiKeyNameMalformedException>()).IsTrue();
    }

    [Test]
    public async Task GetByUserAsync_ReturnsOnlyOwnKeys()
    {
        var user1 = Context.BuildTestUser();
        var user2 = Context.BuildTestUser();

        await _service.CreateAsync(user1.Id, $"User1Key-{TestId}", null);
        await _service.CreateAsync(user2.Id, $"User2Key-{TestId}", null);

        var result = await _service.GetByUserAsync(user1.Id);

        await Assert.That(result.Value!.Count).IsEqualTo(1);
        await Assert.That(result.Value!.First().UserId).IsEqualTo(user1.Id);
    }

    [Test]
    public async Task DeleteAsync_RemovesKey()
    {
        var user = Context.BuildTestUser();
        await _service.CreateAsync(user.Id, $"Delete-{TestId}", null);
        var listed = await _service.GetByUserAsync(user.Id);
        var keyId = listed.Value!.First().Id;

        var result = await _service.DeleteAsync(user.Id, keyId);

        await Assert.That(result.HasError).IsFalse();
        var stillThere = await Context.ApiKeys.AnyAsync(k => k.Id == keyId);
        await Assert.That(stillThere).IsFalse();
    }

    [Test]
    public async Task DeleteAsync_ReturnsError_WhenKeyBelongsToOtherUser()
    {
        var owner = Context.BuildTestUser();
        var attacker = Context.BuildTestUser();
        await _service.CreateAsync(owner.Id, $"Owner-{TestId}", null);
        var listed = await _service.GetByUserAsync(owner.Id);
        var keyId = listed.Value!.First().Id;

        var result = await _service.DeleteAsync(attacker.Id, keyId);

        await Assert.That(result.HasErrorOfType<KeyNotFoundException>()).IsTrue();
        var stillThere = await Context.ApiKeys.AnyAsync(k => k.Id == keyId);
        await Assert.That(stillThere).IsTrue();
    }

}