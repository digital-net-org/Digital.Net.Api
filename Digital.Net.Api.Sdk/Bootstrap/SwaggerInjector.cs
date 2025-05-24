using Digital.Net.Api.Core.Environment;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Digital.Net.Api.Sdk.Bootstrap;

public static class SwaggerInjector
{
    public static WebApplicationBuilder AddSwagger(
        this WebApplicationBuilder builder,
        string applicationName,
        string version
    )
    {
        if (!AspNetEnv.IsDevelopment)
            return builder;

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(version, new OpenApiInfo { Title = applicationName, Version = version });
            c.EnableAnnotations();
            c.OrderActionsBy(
                apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
            c.DocInclusionPredicate((_, api) =>
            {
                if (!api.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller))
                    return true;

                if (controller is not null && controller.EndsWith("Pagination"))
                    api.ActionDescriptor.EndpointMetadata.Add(
                        new SwaggerOperationAttribute { Tags = [controller.Replace("Pagination", string.Empty)] }
                    );

                return true;
            });
        });
        return builder;
    }

    public static IApplicationBuilder UseSwaggerPage(
        this IApplicationBuilder app,
        string applicationName,
        string version
    )
    {
        if (AspNetEnv.IsDevelopment)
        {
            app.UseSwagger(opts => { opts.SerializeAsV2 = true; });
            app.UseSwaggerUI(opts =>
            {
                opts.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"{applicationName} API {version}");
                opts.RoutePrefix = "swagger";
            });
        }
        return app;
    }
}