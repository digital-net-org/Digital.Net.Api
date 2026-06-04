using System;
using System.Net.Http;
using System.Threading.Tasks;
using Digital.Net.Core.Http.Endpoints.Dto;
using Digital.Net.Tests.Core.Http;

namespace Digital.Net.Tests.Core.Sdk;

public static class EventsApi
{
    public const string BaseUrl = "/events";

    public static async Task<HttpResponseMessage> GetEventById(this HttpClient client, Guid id) =>
        await client.GetAsync($"{BaseUrl}/{id}");

    public static async Task<HttpResponseMessage> GetEvents(this HttpClient client, EventQuery? query = null) =>
        await client.GetAsync($"{BaseUrl}{query?.ToQueryString()}");
}
