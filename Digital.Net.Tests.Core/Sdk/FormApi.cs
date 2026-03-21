using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Digital.Net.Cms.Endpoints.Dto;
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

    public static async Task<HttpResponseMessage> CreateForm(this HttpClient client, FormPayload payload) =>
        await client.PostAsJsonAsync(BaseUrl, payload);

    public static async Task<HttpResponseMessage> PatchForm(this HttpClient client, Guid id, object patch) =>
        await client.PatchAsJsonAsync($"{BaseUrl}/{id}", patch);

    public static async Task<HttpResponseMessage> DeleteForm(this HttpClient client, Guid id) =>
        await client.DeleteAsync($"{BaseUrl}/{id}");

    public static async Task<HttpResponseMessage> GetFormFields(this HttpClient client, Guid formId) =>
        await client.GetAsync($"{BaseUrl}/{formId}/fields");

    public static async Task<HttpResponseMessage> GetFormFieldById(this HttpClient client, Guid formId, Guid fieldId) =>
        await client.GetAsync($"{BaseUrl}/{formId}/fields/{fieldId}");

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

    public static async Task<HttpResponseMessage> GetFormSubmission(this HttpClient client, Guid id) =>
        await client.GetAsync($"{SubmissionsUrl}/{id}");

    public static async Task<HttpResponseMessage> DeleteFormSubmission(this HttpClient client, Guid id) =>
        await client.DeleteAsync($"{SubmissionsUrl}/{id}");

    public static async Task<HttpResponseMessage> GetFormDefinition(this HttpClient client, Guid id) =>
        await client.GetAsync($"{BaseUrl}/{id}/definition");

    public static async Task<HttpResponseMessage> SubmitForm(
        this HttpClient client,
        Guid id,
        FormSubmitPayload payload
    ) =>
        await client.PostAsJsonAsync($"{BaseUrl}/{id}/submit", payload);
}
