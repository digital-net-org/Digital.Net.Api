using System.Net;
using System.Net.Http.Json;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Http.Endpoints.Dto;
using Digital.Net.Core.Http.Services.Pagination;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Tests.Core.Http.Security;

public class OrderByHardeningTest
{
    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture ApplicationFixture { get; init; }

    private async Task<(User, HttpClient)> CreateAuthenticatedUserAsync(TestUserPayload? payload = null)
    {
        var user = ApplicationFixture.CreateUser(payload ?? new TestUserPayload { IsActive = true });
        var client = ApplicationFixture.CreateClient();
        await client.Login(user);
        return (user, client);
    }

    [Test]
    public async Task GetUsers_OrderBySecretColumn_ShouldReturnBadRequest_ForAdmin()
    {
        var (_, client) = await CreateAuthenticatedUserAsync(new TestUserPayload { IsAdmin = true, IsActive = true });
        var response = await client.GetAsync("/user?Index=1&Size=10&orderBy=Password");
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);

        response = await client.GetAsync("/user?Index=1&Size=10&orderBy=password&order=asc");
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetUsers_OrderByUnknownColumn_ShouldReturnBadRequest()
    {
        var (_, client) = await CreateAuthenticatedUserAsync(new TestUserPayload { IsAdmin = true, IsActive = true });
        var response = await client.GetAsync("/user?Index=1&Size=10&orderBy=NotAColumn");
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetUsers_OrderBySecretColumn_ShouldBeUnreachable_ForStandardUser()
    {
        var (_, client) = await CreateAuthenticatedUserAsync();
        var response = await client.GetAsync("/user?Index=1&Size=10&orderBy=Password");
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetUsers_OrderByWhitelistedColumn_ShouldSortAscendingAndDescending()
    {
        var (_, client) = await CreateAuthenticatedUserAsync(new TestUserPayload { IsAdmin = true, IsActive = true });
        for (var i = 0; i < 3; i++)
            ApplicationFixture.CreateUser();

        var ascending = await GetUsersAsync(client, "/user?Index=1&Size=50&orderBy=CreatedAt&order=asc");
        await Assert.That(IsSorted(ascending, false)).IsTrue();
        var descending = await GetUsersAsync(client, "/user?Index=1&Size=50&orderBy=CreatedAt&order=desc");
        await Assert.That(IsSorted(descending, true)).IsTrue();
        var response = await client.GetAsync("/user?Index=1&Size=50&orderBy=Login&order=asc");
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    private static async Task<List<UserListDto>> GetUsersAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<QueryResult<UserListDto>>();
        return result!.Value!.ToList();
    }

    private static bool IsSorted(List<UserListDto> users, bool descending)
    {
        var dates = users.Select(u => u.CreatedAt).ToList();
        var sorted = descending
            ? dates.OrderByDescending(d => d).ToList()
            : dates.OrderBy(d => d).ToList();
        return dates.SequenceEqual(sorted);
    }
}