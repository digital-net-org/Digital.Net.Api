using Microsoft.Extensions.DependencyInjection;

namespace Digital.Net.Cms.Services.Articles;

public static class ArticleInjector
{
    public static IServiceCollection AddArticleDependencies(this IServiceCollection services) =>
        services.AddScoped<ArticleService>();
}
