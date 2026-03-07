using Digital.Net.Core.Random;
using Digital.Net.Entities.Models.Pages;
using Digital.Net.Entities.Context;

namespace Digital.Net.Tests.Core.Factories.Data;

public static class TestPageFactory
{
    public static Page BuildTestPage(
        this DigitalContext context,
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
        context.Pages.Add(page);
        context.SaveChanges();
        return page;
    }
}
