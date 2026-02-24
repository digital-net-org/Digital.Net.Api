using Digital.Net.Api.Core.Random;
using Digital.Net.Api.Entities.Models.Pages;
using Digital.Net.Api.Entities.Repositories;

namespace Digital.Net.Tests.Core.Factories.Data;

public static class TestPageFactory
{
    public static Page BuildTestPage(
        this IRepository<Page> pageRepository,
        string? title = null,
        string? path = null,
        bool isPublished = false
    )
    {
        var page = new Page
        {
            Title = title ?? Randomizer.GenerateRandomString(Randomizer.AnyLetter, 20),
            Path = path ?? $"/{Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 10)}",
            Description = "Test page description",
            IsPublished = isPublished
        };
        pageRepository.Create(page);
        pageRepository.Save();
        return page;
    }
}
