using Digital.Net.Api.Controllers.Generic.Pagination;
using Digital.Net.Api.Controllers.Test.TestUtilities.Context;
using Digital.Net.Api.Controllers.Test.TestUtilities.Controllers;
using Digital.Net.Api.Core.Interval;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.TestUtilities;
using Digital.Net.Api.TestUtilities.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace Digital.Net.Api.Controllers.Test.Generic.Pagination;

public class PaginationControllerTest : UnitTest, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PaginationControllerWithId _paginationController;
    private readonly Repository<TestIdEntity, MvcTestContext> _testEntityRepository;

    public PaginationControllerTest()
    {
        _connection = SqliteInMemoryHelper.GetConnection();
        var context = _connection.CreateContext<MvcTestContext>();
        _testEntityRepository = new Repository<TestIdEntity, MvcTestContext>(context);
        _paginationController = new PaginationControllerWithId(_testEntityRepository);
    }

    private QueryResult<TestIdEntityDto> Test(TestIdEntityQuery query)
    {
        var actionResult = _paginationController.Get(query).Result as OkObjectResult;
        return actionResult?.Value as QueryResult<TestIdEntityDto> ?? new QueryResult<TestIdEntityDto>();
    }

    [Fact]
    public void Get_ReturnsMappedModelWithCorrectPagination_WhenQueryIsValid()
    {
        const int total = 10;
        const int index = 1;
        const int size = 5;
        CreateDataPool(total);
        var result = Test(new TestIdEntityQuery { Index = index, Size = size });

        Assert.Equal(total, result.Total);
        Assert.Equal(size, result.Size);
        Assert.Equal(index, result.Index);
        Assert.Equal(size, result.Count);
    }

    [Fact]
    public void Get_ReturnsCorrectItems_WhenFilteredWithMutationDates()
    {
        for (var i = 1; i < 3; i++)
            _testEntityRepository.CreateAndSave(new TestIdEntity { CreatedAt = DateTime.UtcNow.AddDays(-i + 1) });

        var result = Test(new TestIdEntityQuery { CreatedAt = DateTime.UtcNow.AddDays(-1) });
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Get_ReturnsCorrectItems_WhenFilteredWithMutationDateRanges()
    {
        var now = DateTime.UtcNow;
        var users = CreateDataPool(5);
        foreach (var user in users)
        {
            var i = users.IndexOf(user);
            user.CreatedAt = now.AddDays(i);
            _testEntityRepository.Save();
        }

        var result = Test(new TestIdEntityQuery { CreatedIn = new DateRange { From = now, To = now.AddDays(2) } });
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Get_ReturnsCorrectItems_WhenIndexInSecondPage()
    {
        const int total = 10;
        const int index = 2;
        const int size = 5;
        var users = CreateDataPool(total);
        foreach (var user in users)
        {
            var i = users.IndexOf(user) + 1;
            user.Username = $"User{i}";
            user.CreatedAt = DateTime.UtcNow.AddDays(-total + i);
            _testEntityRepository.Save();
        }

        var result = Test(new TestIdEntityQuery { Index = index, Size = size, OrderBy = "CreatedAt" });
        Assert.Equal("User6", result.Value.First().Username);
    }

    [Fact]
    public void Get_ReturnsError_WhenInvalidOrder()
    {
        CreateDataPool(10);
        var result = Test(new TestIdEntityQuery { OrderBy = "Lol" });
        Assert.True(result.HasError);
        Assert.NotEmpty(result.Errors);
    }

    private List<TestIdEntity> CreateDataPool(int count)
    {
            var entities = new List<TestIdEntity>();
            for (var i = 0; i < count; i++)
                entities.Add(_testEntityRepository.CreateAndSave(new TestIdEntity()));
            return entities;
    }

    public void Dispose() => _connection.Dispose();
}