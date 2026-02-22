using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi;

namespace Digital.Net.Api.Core.OpenApi;

public static class OpenApiExtensions
{
    /// <summary>
    ///     Adds documentation to a route handler by configuring its OpenAPI operation.
    /// </summary>
    /// <param name="builder">The route handler builder to which the documentation will be added.</param>
    /// <param name="configureOperation">An action to configure the OpenAPI operation associated with the route handler.</param>
    /// <returns>The modified route handler builder with the OpenAPI operation configured.</returns>
    public static RouteHandlerBuilder WithDoc(
        this RouteHandlerBuilder builder,
        Action<OpenApiOperation> configureOperation) =>
        builder.AddOpenApiOperationTransformer((operation, _, _) =>
        {
            configureOperation(operation);
            return Task.CompletedTask;
        });
}