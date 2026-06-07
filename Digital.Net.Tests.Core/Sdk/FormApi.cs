using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Digital.Net.Cms.Http.Dto;
using Digital.Net.Tests.Core.Http;

namespace Digital.Net.Tests.Core.Sdk;

public static class FormApi
{
    public const string BaseUrl = "/cms/forms";
    public const string SubmissionsUrl = "/cms/forms/submissions";

    public static async Task<HttpResponseMessage> GetFormById(this HttpClient client, Guid id) =>
        await client.GetAsync($"{BaseUrl}/{id}");

    public static async Task<HttpResponseMessage> GetForms(this HttpClient client, FormQuery? query = null) =>
        await client.GetAsync($"{BaseUrl}{query?.ToQueryString()}");

    public static async Task<HttpResponseMessage> CreateForm(this HttpClient client, FormCreatePayload createPayload) =>
        await client.PostAsJsonAsync(BaseUrl, createPayload);

    public static async Task<HttpResponseMessage> DeleteForm(this HttpClient client, Guid id) =>
        await client.DeleteAsync($"{BaseUrl}/{id}");

    public static async Task<HttpResponseMessage> CreateFormField(
        this HttpClient client,
        Guid formId,
        FormFieldPayload payload
    ) =>
        await client.PostAsJsonAsync($"{BaseUrl}/{formId}/fields", payload);

    public static async Task<HttpResponseMessage> PatchFormField(
        this HttpClient client,
        Guid formId,
        Guid fieldId,
        object patch
    ) =>
        await client.PatchAsJsonAsync($"{BaseUrl}/{formId}/fields/{fieldId}", patch);

    public static async Task<HttpResponseMessage> DeleteFormField(
        this HttpClient client,
        Guid formId,
        Guid fieldId
    ) =>
        await client.DeleteAsync($"{BaseUrl}/{formId}/fields/{fieldId}");

    public static async Task<HttpResponseMessage> GetFormSubmissions(
        this HttpClient client,
        FormSubmissionQuery? query = null
    ) =>
        await client.GetAsync($"{SubmissionsUrl}{query?.ToQueryString()}");
}