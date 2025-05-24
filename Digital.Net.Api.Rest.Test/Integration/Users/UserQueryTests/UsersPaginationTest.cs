using System.Net.Http.Json;
using Digital.Net.Api.Controllers.Controllers.UserApi.Dto;
using Digital.Net.Api.Controllers.Generic.Pagination;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Rest.Test.Api;
using Digital.Net.Api.TestUtilities.Integration;

namespace Digital.Net.Api.Rest.Test.Integration.Users.UserQueryTests;

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