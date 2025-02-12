using Digital.Lib.Net.Entities.Services;
using Digital.Pages.Services.Documents;
using Digital.Pages.Services.Seeder;
using Digital.Pages.Services.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Digital.Pages.Services;

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