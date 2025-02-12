using System.Net.Http.Json;
using Digital.Lib.Net.Http.Factories;
using SafariDigital.Api.Dto.Entities;
using Tests.Utils.ApiCollections.Models;

namespace Tests.Utils.ApiCollections;

public static class FrameCollection
{
    public static async Task<HttpResponseMessage> GetAllFrames(this HttpClient client) =>
        await client.GetAsync("/frame");

    public static async Task<HttpResponseMessage> CreateFrame(this HttpClient client, FramePayload request) =>
        await client.PostAsync("/frame", JsonContent.Create(request));

    public static async Task<HttpResponseMessage>
        PatchFrame(this HttpClient client, Guid id, PatchFramePayload request) =>
        await client.PatchAsync($"/frame/{id}", BodyFactory.BuildPatch(request));
}