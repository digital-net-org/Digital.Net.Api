using Digital.Net.Entities.Repositories;
using Digital.Net.Entities.Services;
using Microsoft.Extensions.DependencyInjection;
using SafariDigital.Data.Context;
using SafariDigital.Data.Models;
using SafariDigital.Data.Models.Database.Frames;
using SafariDigital.Data.Models.Database.Users;
using SafariDigital.Data.Models.Database.Views;

namespace SafariDigital.Data;

public static class DataInjector
{
    public static IServiceCollection AddRepositories(this IServiceCollection services) =>
        services
            .AddScoped(typeof(IRepository<>), typeof(SafariDigitalRepository<>))
            .AddScoped<ISeeder, Seeder>()
            .AddScoped<IEntityService<User, UserQuery>, UserEntityService>()
            .AddScoped<IEntityService<View, ViewQuery>, ViewEntityService>()
            .AddScoped<IEntityService<Frame, FrameQuery>, FrameEntityService>();
}