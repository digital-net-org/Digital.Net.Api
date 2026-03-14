using System.Net;
using System.Net.Http.Json;
using Digital.Net.Api.Endpoints.Dto;
using Digital.Net.Api.Services.Authentication.Events;
using Digital.Net.Core.Messages;
using Digital.Net.Entities.Crud.Enpoints;
using Digital.Net.Entities.Models.Events;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Api.Test.Endpoints;

public class EventsEndpointsTest
{
    [ClassDataSource<TestApplication>]
    public required TestApplication Application { get; init; }

    private async Task<(Entities.Models.Users.User, HttpClient)> CreateTestAdminAsync()
    {
        var admin = Application.CreateUser(new TestUserPayload { IsActive = true, IsAdmin = true });
        var client = Application.CreateClient();
        await client.Login(admin);
        return (admin, client);
    }

    private async Task<Event> CreateEventAsync(Guid? userId = null, string name = AuthenticationEvents.Login)
    {
        var context = Application.GetContext();
        var e = new Event
        {
            Name = name,
            UserId = userId,
            UserAgent = "test-agent",
            IpAddress = "127.0.0.1",
            State = EventState.Success
        };
        await context.Events.AddAsync(e);
        await context.SaveChangesAsync();
        return e;
    }

    [Test]
    public async Task GetEvents_ShouldReturnPaginatedEvents()
    {
        var (_, client) = await CreateTestAdminAsync();
        await CreateEventAsync();
        await CreateEventAsync();

        var response = await client.GetEvents(new EventQuery());

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<QueryResult<EventDto>>();
        await Assert.That(result!.Total).IsGreaterThanOrEqualTo(2);
    }

    [Test]
    public async Task GetEvents_ShouldFilterByUserId()
    {
        var (_, client) = await CreateTestAdminAsync();
        var user = Application.CreateUser();
        var otherUser = Application.CreateUser();

        await CreateEventAsync(user.Id);
        await CreateEventAsync(otherUser.Id);

        var response = await client.GetEvents(new EventQuery { UserId = user.Id });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<EventDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value.All(e => e.UserId == user.Id)).IsTrue();
    }

    [Test]
    public async Task GetEvents_ShouldFilterByEventType()
    {
        var (_, client) = await CreateTestAdminAsync();
        await CreateEventAsync(name: AuthenticationEvents.Login);
        await CreateEventAsync(name: AuthenticationEvents.Logout);

        var response = await client.GetEvents(new EventQuery { EventType = AuthenticationEvents.Login });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<EventDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value.All(e => e.Name == AuthenticationEvents.Login)).IsTrue();
    }

    [Test]
    public async Task GetEvents_ShouldFilterByDateRange()
    {
        var (_, client) = await CreateTestAdminAsync();
        await CreateEventAsync();

        var from = DateTime.UtcNow.AddDays(-1);
        var to = DateTime.UtcNow.AddDays(1);

        var response = await client.GetEvents(new EventQuery { CreatedFrom = from, CreatedTo = to });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<EventDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Total).IsGreaterThanOrEqualTo(1);
    }

    [Test]
    public async Task GetEventById_ShouldReturnEvent()
    {
        var (_, client) = await CreateTestAdminAsync();
        var e = await CreateEventAsync();

        var response = await client.GetEventById(e.Id);
        var result = await response.Content.ReadFromJsonAsync<Result<EventDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value!.Id).IsEqualTo(e.Id);
        await Assert.That(result.Value.Name).IsEqualTo(e.Name);
    }

    [Test]
    public async Task GetEventById_ShouldReturnNotFound_OnUnknownId()
    {
        var (_, client) = await CreateTestAdminAsync();
        var response = await client.GetEventById(Guid.NewGuid());
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetEvents_ShouldBeAdminOnly()
    {
        var user = Application.CreateUser(new TestUserPayload { IsActive = true, IsAdmin = false });
        var client = Application.CreateClient();
        await client.Login(user);

        var response = await client.GetEvents();
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Forbidden);
    }
}
