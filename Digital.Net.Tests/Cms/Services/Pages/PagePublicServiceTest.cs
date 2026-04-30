using Digital.Net.Cms.Context;
using Digital.Net.Cms.Services.Pages;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Tests.Core;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using TUnit.Core.Interfaces;

namespace Digital.Net.Tests.Cms.Services.Pages;

public class PagePublicServiceTest : UnitTest, IAsyncInitializer
{
    [ClassDataSource<DatabaseFixture>]
    public required DatabaseFixture DbFixture { get; init; }

    private CmsContext _context = null!;
    private PagePublicService _service = null!;

    public async Task InitializeAsync()
    {
        await DbFixture.EnsureCreatedAsync<CmsContext>();
        _context = DbFixture.CreateContext<CmsContext>();
        _service = new PagePublicService(_context);
    }

    [Test]
    public async Task GetPageByPath_ShouldReturnPublishedPage()
    {
        var path = "/test-public-" + Guid.NewGuid().ToString("N")[..8];
        _context.BuildTestPage(path, true);
        var result = await _service.GetPageByPath(path);

        await Assert.That(result.HasError).IsFalse();
        await Assert.That(result.Value).IsNotNull();
    }

    [Test]
    public async Task GetPageByPath_ShouldReturnNotFound_WhenPageIsNotPublished()
    {
        var path = "/test-unpublished-" + Guid.NewGuid().ToString("N")[..8];
        _context.BuildTestPage(path);
        var result = await _service.GetPageByPath(path);
        await Assert.That(result.HasErrorOfType<ResourceNotFoundException>()).IsTrue();
    }

    [Test]
    public async Task GetPageByPath_ShouldReturnNotFound_WhenPageDoesNotExist()
    {
        var path = "/missing-" + Guid.NewGuid().ToString("N")[..8];
        var result = await _service.GetPageByPath(path);
        await Assert.That(result.HasErrorOfType<ResourceNotFoundException>()).IsTrue();
    }
}