using Microsoft.Extensions.DependencyInjection;
using Safari.Net.Data.Entities;
using Safari.Net.Data.Repositories;
using SafariDigital.Data.Context;
using SafariDigital.Data.Models;
using SafariDigital.Data.Models.Database;
using SafariDigital.Data.Services;

namespace SafariDigital.Data;

public static class DataInjector
{
    public static IServiceCollection AddRepositories(this IServiceCollection services) =>
        services
            .AddScoped(typeof(IRepository<>), typeof(SafariDigitalRepository<>))
            .AddScoped<ISeeder, Seeder>()
            .AddScoped<IEntityService<User, UserQuery>, UserEntityService>()
            .AddScoped<IEntityService<View, ViewQuery>, ViewEntityService>()
            .AddScoped<IEntityService<ViewFrame, ViewFrameQuery>, ViewFrameEntityService>();
}