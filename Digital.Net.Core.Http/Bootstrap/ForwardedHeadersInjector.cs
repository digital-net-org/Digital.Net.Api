using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IPNetwork = System.Net.IPNetwork;

namespace Digital.Net.Core.Http.Bootstrap;

public static class ForwardedHeadersInjector
{
    public static WebApplicationBuilder SetForwardedHeaders(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            var proxies = configuration.GetTrustedEntries(CoreSettings.ForwardedHeadersKnownProxiesKey);
            var networks = configuration.GetTrustedEntries(CoreSettings.ForwardedHeadersKnownIPNetworksKey);

            options.ForwardedHeaders = proxies.Length is 0 && networks.Length is 0
                ? ForwardedHeaders.None
                : ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = configuration.GetValue<int?>(CoreSettings.ForwardedHeadersForwardLimitKey)
                                   ?? CoreSettings.DefaultForwardLimit;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();

            foreach (var proxy in proxies)
                options.KnownProxies.Add(
                    IPAddress.TryParse(proxy, out var address)
                        ? address
                        : throw InvalidEntry(CoreSettings.ForwardedHeadersKnownProxiesKey, proxy)
                );
            foreach (var network in networks)
                options.KnownIPNetworks.Add(
                    IPNetwork.TryParse(network, out var ipNetwork)
                        ? ipNetwork
                        : throw InvalidEntry(CoreSettings.ForwardedHeadersKnownIPNetworksKey, network)
                );
        });
        return builder;
    }

    private static string[] GetTrustedEntries(this IConfiguration configuration, string key) =>
        configuration.GetSection(key).Get<string[]>() ?? [];

    private static InvalidOperationException InvalidEntry(string key, string value) =>
        new($"Invalid value '{value}' in configuration '{key}'.");
}
