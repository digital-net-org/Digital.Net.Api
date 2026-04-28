using System.Net;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.Services.Events;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Cms.Test.Endpoints;

public class SseStreamEndpointsTest
{
    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture ApplicationFixture { get; init; }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var user = ApplicationFixture.CreateUser(new TestUserPayload { IsActive = true });
        var client = ApplicationFixture.CreateClient();
        await client.Login(user);
        return client;
    }

    [Test]
    public async Task Subscribe_ShouldRequireApplicationAuth()
    {
        var client = ApplicationFixture.CreateClient();

        var response = await client.GetAsync(SseApi.CmsStreamUrl);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Signal_ShouldBeEmitted_WhenCmsEntityCreated()
    {
        EventSignal? received = null;
        var signalService = ApplicationFixture.GetService<IEventSignalService>();
        signalService.OnSignal += signal => received = signal;

        var apiClient = await CreateAuthenticatedClientAsync();
        await apiClient.CreateTag(new TagPayload { Name = "signal-test", Color = "#000" });

        await Assert.That(received).IsNotNull();
        await Assert.That(received!.Name).IsEqualTo("CMS_CREATE_TAG");
        await Assert.That(received.State).IsEqualTo(EventState.Success);
    }

    [Test]
    public async Task Signal_ShouldBeEmittedWithFailedState_WhenOperationFails()
    {
        EventSignal? received = null;
        var signalService = ApplicationFixture.GetService<IEventSignalService>();
        signalService.OnSignal += signal => received = signal;

        var apiClient = await CreateAuthenticatedClientAsync();
        // Delete a non-existent entity — will fail
        await apiClient.DeleteTag(Guid.NewGuid());

        await Assert.That(received).IsNotNull();
        await Assert.That(received!.Name).IsEqualTo("CMS_DELETE_TAG");
        await Assert.That(received.State).IsEqualTo(EventState.Failed);
    }

    [Test]
    public async Task Signal_ShouldPassCmsFilter_WhenSuccessful()
    {
        Func<EventSignal, bool> cmsFilter = signal =>
            signal.Name.StartsWith("CMS_") && signal.State == EventState.Success;

        EventSignal? received = null;
        var signalService = ApplicationFixture.GetService<IEventSignalService>();
        signalService.OnSignal += signal =>
        {
            if (cmsFilter(signal))
                received = signal;
        };

        var apiClient = await CreateAuthenticatedClientAsync();
        await apiClient.CreateTag(new TagPayload { Name = "filter-test", Color = "#111" });

        await Assert.That(received).IsNotNull();
        await Assert.That(received!.Name).IsEqualTo("CMS_CREATE_TAG");
    }

    [Test]
    public async Task Signal_ShouldNotPassCmsFilter_WhenFailed()
    {
        Func<EventSignal, bool> cmsFilter = signal =>
            signal.Name.StartsWith("CMS_") && signal.State == EventState.Success;

        EventSignal? received = null;
        var signalService = ApplicationFixture.GetService<IEventSignalService>();
        signalService.OnSignal += signal =>
        {
            if (cmsFilter(signal))
                received = signal;
        };

        var apiClient = await CreateAuthenticatedClientAsync();
        await apiClient.DeleteTag(Guid.NewGuid());

        await Assert.That(received).IsNull();
    }

    [Test]
    public async Task Signal_ShouldNotPassCmsFilter_WhenNotCmsEvent()
    {
        Func<EventSignal, bool> cmsFilter = signal =>
            signal.Name.StartsWith("CMS_") && signal.State == EventState.Success;

        EventSignal? received = null;
        var signalService = ApplicationFixture.GetService<IEventSignalService>();
        signalService.OnSignal += signal =>
        {
            if (cmsFilter(signal))
                received = signal;
        };

        // Login triggers AUTH_LOGIN — not a CMS event
        var user = ApplicationFixture.CreateUser(new TestUserPayload { IsActive = true });
        var client = ApplicationFixture.CreateClient();
        await client.Login(user);

        await Assert.That(received).IsNull();
    }
}
