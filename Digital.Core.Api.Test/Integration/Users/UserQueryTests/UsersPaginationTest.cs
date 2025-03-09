using System.Net.Http.Json;
using Digital.Core.Api.Controllers.UserApi.Dto;
using Digital.Core.Api.Test.Api;
using Digital.Lib.Net.Entities.Models.Users;
using Digital.Lib.Net.Mvc.Controllers.Pagination;
using Digital.Lib.Net.TestTools.Integration;

namespace Digital.Core.Api.Test.Integration.Users.UserQueryTests;

public class UsersPaginationTest(AppFactory<Program> fixture) : UsersTest(fixture)
{
    [Fact]
    public async Task GetUser_WithoutFilters_ReturnsAllRows() =>
        Assert.Equal(UserPool.Count, (await ExecuteQuery()).Value.Count());

    [Fact]
    public async Task GetUser_WithStateFilter_ReturnsCorrespondingRows() =>
        Assert.Equal(1, (await ExecuteQuery(new UserQuery { IsActive = false })).Count);

    [Fact]
    public async Task GetUser_WithUsernameFilter_ReturnsCorrespondingRows() =>
        Assert.Equal(7, (await ExecuteQuery(new UserQuery { Username = "user1" })).Count);

    private async Task<QueryResult<UserDto>> ExecuteQuery(UserQuery? query = null)
    {
        await BaseClient.Login(UserRepository.Get().First());
        var response = await BaseClient.GetUsers(query ?? new UserQuery());
        return await response.Content.ReadFromJsonAsync<QueryResult<UserDto>>() ?? throw new Exception();
    }
}