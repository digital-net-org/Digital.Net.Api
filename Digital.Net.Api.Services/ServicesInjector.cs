using Digital.Net.Api.Services.Application;
using Digital.Net.Api.Services.Authentication;
using Digital.Net.Api.Services.Documents;
using Digital.Net.Api.Services.Events;
using Digital.Net.Api.Services.HttpContext;
using Digital.Net.Api.Services.Options;
using Digital.Net.Api.Services.Pages;
using Digital.Net.Api.Services.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Api.Services;

public static class ServicesInjector
{
    public static IServiceCollection AddDigitalNetServices(this IServiceCollection services)
    {
        services
            .AddDigitalApplicationServices()
            .AddDigitalOptionsServices()
            .AddDigitalAuthenticationServices()
            .AddDigitalHttpContextServices()
            .AddDigitalEventServices()
            .AddDigitalUserServices()
            .AddDigitalDocumentServices()
            .AddDigitalPageServices();
        return services;
    }
    
    public static WebApplicationBuilder AddDigitalNetServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddDigitalNetServices();
        return builder;
    }
}