using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Tests.Core.Http;

namespace Digital.Net.Tests.Core.Sdk;

public static class SheetApi
{
    public const string BaseUrl = "/cms/sheets";

    public static async Task<HttpResponseMessage> GetSheetById(this HttpClient client, Guid sheetId) =>
        await client.GetAsync($"{BaseUrl}/{sheetId}");

    public static async Task<HttpResponseMessage> GetSheets(this HttpClient client, SheetQuery? query = null) =>
        await client.GetAsync($"{BaseUrl}{query?.ToQueryString()}");

    public static async Task<HttpResponseMessage> CreateSheet(this HttpClient client, SheetPayload payload) =>
        await client.PostAsJsonAsync(BaseUrl, payload);

    public static async Task<HttpResponseMessage> PatchSheet(this HttpClient client, Guid sheetId, object patch) =>
        await client.PatchAsJsonAsync($"{BaseUrl}/{sheetId}", patch);

    public static async Task<HttpResponseMessage> DeleteSheet(this HttpClient client, Guid sheetId) =>
        await client.DeleteAsync($"{BaseUrl}/{sheetId}");

    public static async Task<HttpResponseMessage> GetPageSheets(this HttpClient client, Guid pageId) =>
        await client.GetAsync($"/cms/pages/{pageId}/sheets");

    public static async Task<HttpResponseMessage> AssociateSheet(
        this HttpClient client,
        Guid pageId,
        PageSheetPayload payload
    ) =>
        await client.PostAsJsonAsync($"/cms/pages/{pageId}/sheets", payload);

    public static async Task<HttpResponseMessage> DissociateSheet(
        this HttpClient client,
        Guid pageId,
        Guid sheetId
    ) =>
        await client.DeleteAsync($"/cms/pages/{pageId}/sheets/{sheetId}");

    public static async Task<HttpResponseMessage> GetResource(this HttpClient client, Guid sheetId) =>
        await client.GetAsync($"/cms/resource/{sheetId}");
}
