using Digital.Lib.Net.Entities.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SafariDigital.Services.Documents;
using SafariDigital.Services.Seeder;
using SafariDigital.Services.Users;

namespace SafariDigital.Services;

public static class Injector
{
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped(typeof(ISeeder<>), typeof(Seeder<>));
        builder.Services.AddScoped<ISeederService, SeederService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IDocumentService, DocumentService>();
        return builder;
    }
}