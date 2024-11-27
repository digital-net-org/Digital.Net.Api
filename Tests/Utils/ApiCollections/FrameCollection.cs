using System.Net.Http.Json;
using SafariDigital.Services.Frames.Models;

namespace Tests.Utils.ApiCollections;

public static class FrameCollection
{
    public static async Task<HttpResponseMessage> GetAllFrames(this HttpClient client) =>
        await client.GetAsync("/frame");

    public static async Task<HttpResponseMessage> CreateFrame(this HttpClient client, CreateFrameRequest request) =>
        await client.PostAsync("/frame", JsonContent.Create(request));
}