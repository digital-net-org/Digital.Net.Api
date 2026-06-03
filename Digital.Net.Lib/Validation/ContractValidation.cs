using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Lib.Validation;

public static class ContractValidation
{
    public static IServiceCollection RequireContract<TContract>(this IServiceCollection services, string providerHint)
        where TContract : class
        => services.AddSingleton(new RequiredContract(typeof(TContract), providerHint));

    public static void ValidateRequiredContracts(this IServiceProvider services)
    {
        var inspector = services.GetRequiredService<IServiceProviderIsService>();
        var missing = services
            .GetServices<RequiredContract>()
            .Where(contract => !inspector.IsService(contract.ContractType))
            .ToList();

        if (missing.Count == 0)
            return;

        var details = string.Join(
            '\n',
            missing.Select(m => $"  - {m.ContractType.Name} (register via {m.ProviderHint})")
        );

        throw new InvalidOperationException(
            $"Digital.Net startup: missing required contract implementation(s):\n{details}"
        );
    }
}