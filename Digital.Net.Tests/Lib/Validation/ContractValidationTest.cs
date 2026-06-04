using Digital.Net.Lib.Validation;
using Digital.Net.Tests.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Tests.Lib.Validation;

public class ContractValidationTest : UnitTest
{
    // Throwaway contract + implementation, local to this test.
    private interface ISampleContract;

    private sealed class SampleImpl : ISampleContract;

    [Test]
    public async Task ValidateRequiredContracts_Throws_WhenContractHasNoImplementation()
    {
        var services = new ServiceCollection();
        services.RequireContract<ISampleContract>("AddSampleLayer");
        var provider = services.BuildServiceProvider();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            provider.ValidateRequiredContracts();
            await Task.CompletedTask;
        });
    }

    [Test]
    public async Task ValidateRequiredContracts_DoesNotThrow_WhenContractIsRegistered()
    {
        var services = new ServiceCollection();
        services.RequireContract<ISampleContract>("AddSampleLayer");
        services.AddScoped<ISampleContract, SampleImpl>();
        var provider = services.BuildServiceProvider();

        await Assert.That(() => provider.ValidateRequiredContracts()).ThrowsNothing();
    }

    [Test]
    public async Task ValidateRequiredContracts_DoesNotThrow_WhenNoContractIsRequired()
    {
        var provider = new ServiceCollection().BuildServiceProvider();
        await Assert.That(() => provider.ValidateRequiredContracts()).ThrowsNothing();
    }

    [Test]
    public async Task ValidateRequiredContracts_Message_NamesMissingContractAndProvider()
    {
        var services = new ServiceCollection();
        services.RequireContract<ISampleContract>("AddSampleLayer");
        var provider = services.BuildServiceProvider();

        var message = string.Empty;
        try
        {
            provider.ValidateRequiredContracts();
        }
        catch (InvalidOperationException ex)
        {
            message = ex.Message;
        }

        await Assert.That(message).Contains(nameof(ISampleContract));
        await Assert.That(message).Contains("AddSampleLayer");
    }
}
