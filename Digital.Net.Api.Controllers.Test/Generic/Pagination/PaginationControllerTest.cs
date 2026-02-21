using Digital.Net.Api.Controllers.Controllers.PageApi.Dto;
using Digital.Net.Api.Controllers.Generic.Pagination;
using Digital.Net.Api.Core.Interval;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Pages;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Tests.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace Digital.Net.Api.Controllers.Test.Generic.Pagination;

public class PaginationControllerTest : UnitTest, IDisposable
{
    private class TestPaginationController(IRepository<Page, DigitalContext> repository)
        : PaginationController<Page, DigitalContext, PageLightDto, PageQuery>(repository);
    
    private readonly SqliteConnection _connection;
    private readonly TestPaginationController _paginationController;
    private readonly Repository<Page, DigitalContext> _testEntityRepository;

    public PaginationControllerTest()
    {
        _connection = SqliteInMemoryHelper.GetConnection();
        var context = _connection.CreateContext<DigitalContext>();
        _testEntityRepository = new Repository<Page, DigitalContext>(context);
        _paginationController = new TestPaginationController(_testEntityRepository);
    }

    private QueryResult<PageLightDto> Test(PageQuery query)
    {
        var actionResult = _paginationController.Get(query).Result as OkObjectResult;
        return actionResult?.Value as QueryResult<PageLightDto> ?? new QueryResult<PageLightDto>();
    }

    [Test]
    public async Task Get_ReturnsMappedModelWithCorrectPagination_WhenQueryIsValid()
    {
        const int total = 10;
        const int index = 1;
        const int size = 5;
        CreateDataPool(total);
        var result = Test(new PageQuery { Index = index, Size = size });

        await Assert.That(result.Total).IsEqualTo(total);
        await Assert.That(result.Size).IsEqualTo(size);
        await Assert.That(result.Index).IsEqualTo(index);
        await Assert.That(result.Count).IsEqualTo(size);
    }

    [Test]
    public async Task Get_ReturnsCorrectItems_WhenFilteredWithMutationDates()
    {
        for (var i = 1; i < 3; i++)
            _testEntityRepository.CreateAndSave(new Page
            {
                CreatedAt = DateTime.UtcNow.AddDays(-i + 1),
                Title = $"Page {i + 1}",
                Description = $"Description for page {i + 1}",
                Path = $"/page/{i + 1}"
            });

        var result = Test(new PageQuery { CreatedAt = DateTime.UtcNow.AddDays(-1) });
        await Assert.That(result.Count).IsEqualTo(2);
    }

    [Test]
    public async Task Get_ReturnsCorrectItems_WhenFilteredWithMutationDateRanges()
    {
        var now = DateTime.UtcNow;
        var users = CreateDataPool(5);
        foreach (var user in users)
        {
            var i = users.IndexOf(user);
            user.CreatedAt = now.AddDays(i);
            _testEntityRepository.Save();
        }

        var result = Test(new PageQuery { CreatedIn = new DateRange { From = now, To = now.AddDays(2) } });
        await Assert.That(result.Count).IsEqualTo(3);
    }

    [Test]
    public async Task Get_ReturnsCorrectItems_WhenIndexInSecondPage()
    {
        const int total = 10;
        const int index = 2;
        const int size = 5;
        var users = CreateDataPool(total);
        foreach (var user in users)
        {
            var i = users.IndexOf(user) + 1;
            user.CreatedAt = DateTime.UtcNow.AddDays(-total + i);
            _testEntityRepository.Save();
        }

        var result = Test(new PageQuery { Index = index, Size = size, OrderBy = "CreatedAt" });
        await Assert.That(result.Value.First().Title).IsEqualTo("Page 6");
    }

    [Test]
    public async Task Get_ReturnsError_WhenInvalidOrder()
    {
        CreateDataPool(10);
        var result = Test(new PageQuery { OrderBy = "Lol" });
        await Assert.That(result.HasError).IsTrue();
        await Assert.That(result.Errors).IsNotEmpty();
    }

    private List<Page> CreateDataPool(int count)
    {
        var entities = new List<Page>();
            for (var i = 0; i < count; i++)
                entities.Add(_testEntityRepository.CreateAndSave(new Page
                {
                    Title = $"Page {i + 1}",
                    Description = $"Description for page {i + 1}",
                    Path = $"/page/{i + 1}"
                }));
            return entities;
    }

    public void Dispose() => _connection.Dispose();
}