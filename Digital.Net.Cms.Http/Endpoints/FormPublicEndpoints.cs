using System.Text.Json;
using Digital.Net.Cms.Context;
using Digital.Net.Cms.Http.Dto;
using Digital.Net.Cms.Models.Forms;
using Digital.Net.Core.Http.Security;
using Digital.Net.Core.Http.Services.Authentication.Filters;
using Digital.Net.Lib.Entities.Projection;
using Digital.Net.Lib.Messages;
using Digital.Net.Lib.String;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Http.Endpoints;

public static class FormPublicEndpoints
{
    private const int MaxFieldValueLength = 4096;
    private const int MaxSerializedValuesLength = 16_384;

    public static IEndpointRouteBuilder MapCmsFormPublicEndpoints(this IEndpointRouteBuilder app)
    {
        var publicApi = app
            .MapGroup("cms/forms/public")
            .WithTags("CMS.FormsPublic")
            .RequireRateLimiting(RateLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Application | AuthorizeType.Jwt | AuthorizeType.ApiKey);

        publicApi
            .MapGet("{id:guid}/definition", GetFormDefinition)
            .WithSummary("GetFormDefinition")
            .WithDescription("Returns the full definition of a published form.");

        publicApi
            .MapGet("by-path", GetFormDefinitionByPath)
            .WithSummary("GetFormDefinitionByPath")
            .WithDescription("Returns the full definition of a published form resolved by its public path (`?path=`).");

        publicApi
            .MapPost("{id:guid}/submit", SubmitForm)
            .WithSummary("SubmitForm")
            .WithDescription("Submits values for a published form. Validates all fields server-side.")
            .RequireAuthentication(AuthorizeType.Application | AuthorizeType.Jwt | AuthorizeType.ApiKey);

        return app;
    }

    private static async Task<Results<Ok<Result<FormPublicDto>>, NotFound>> GetFormDefinition(
        Guid id,
        CmsContext context,
        CancellationToken ct
    )
    {
        var dto = await context.Forms
            .Where(f => f.Id == id && f.Published)
            .ProjectTo<Form, FormPublicDto>()
            .FirstOrDefaultAsync(ct);
        return dto is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(new Result<FormPublicDto>(dto));
    }

    private static async Task<Results<Ok<Result<FormPublicDto>>, NotFound>> GetFormDefinitionByPath(
        [FromQuery] string? path,
        CmsContext context,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(path))
            return TypedResults.NotFound();

        var dto = await context.Forms
            .Where(f => f.Path == path && f.Published)
            .ProjectTo<Form, FormPublicDto>()
            .FirstOrDefaultAsync(ct);
        return dto is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(new Result<FormPublicDto>(dto));
    }

    private static async Task<IResult> SubmitForm(
        Guid id,
        [FromBody]
        FormSubmitPayload payload,
        CmsContext context
    )
    {
        var form = await context.Forms
            .Include(f => f.Fields)
            .FirstOrDefaultAsync(f => f.Id == id && f.Published);

        if (form is null)
            return Results.NotFound();

        var errors = ValidateSubmission(form.Fields, payload.Values);
        if (errors.Count > 0)
        {
            var result = new Result();
            foreach (var error in errors)
                result.AddError(new Exception(error));
            return Results.BadRequest(result);
        }

        var valuesJson = JsonSerializer.Serialize(payload.Values);
        if (valuesJson.Length > MaxSerializedValuesLength)
            return Results.BadRequest(new Result().AddError(new Exception("Submission payload is too large.")));

        var submission = new FormSubmission
        {
            FormId = id,
            ValuesJson = valuesJson,
            SubmitterIp = payload.SubmitterIp,
            UserAgent = payload.UserAgent
        };
        context.FormSubmissions.Add(submission);
        await context.SaveChangesAsync();

        return Results.Ok(new Result());
    }

    private static List<string> ValidateSubmission(List<FormField> fields, Dictionary<string, string?> values)
    {
        var errors = new List<string>();

        var fieldNames = fields.Select(f => f.Name).ToHashSet();
        foreach (var (key, value) in values)
        {
            if (!fieldNames.Contains(key))
                errors.Add($"{key}: Unknown field.");
            if (value is { Length: > MaxFieldValueLength })
                errors.Add($"{key}: Value exceeds {MaxFieldValueLength} characters.");
        }

        foreach (var field in fields.Where(f => f.Type != FormFieldTypes.Message))
        {
            var hasValue = values.TryGetValue(field.Name, out var rawValue) &&
                           !string.IsNullOrWhiteSpace(rawValue);

            if (field.Required && !hasValue)
            {
                errors.Add($"{field.Name}: This field is required.");
                continue;
            }

            if (!hasValue || rawValue is null)
                continue;
            if (field.Type == FormFieldTypes.Email && !RegularExpressions.Email.IsMatch(rawValue))
            {
                errors.Add($"{field.Name}: Invalid email address.");
                continue;
            }

            if (field.Type == FormFieldTypes.Checkbox && rawValue is not "true" and not "false")
            {
                errors.Add($"{field.Name}: Must be 'true' or 'false'.");
                continue;
            }

            if (field.Type is FormFieldTypes.Select or FormFieldTypes.Radio && field.OptionsJson is not null)
            {
                try
                {
                    var options = JsonSerializer.Deserialize<List<string>>(field.OptionsJson);
                    if (options is not null && !options.Contains(rawValue))
                        errors.Add($"{field.Name}: Invalid option '{rawValue}'.");
                }
                catch (JsonException)
                {
                    // Invalid OptionsJson — skip validation
                }

                continue;
            }

            if (field.ValidationJson is null)
                continue;

            try
            {
                var validation = JsonSerializer.Deserialize<JsonElement>(field.ValidationJson);

                if (validation.TryGetProperty("minLength", out var minLengthEl) &&
                    minLengthEl.TryGetInt32(out var minLength) &&
                    rawValue.Length < minLength)
                    errors.Add($"{field.Name}: Must be at least {minLength} characters.");

                if (validation.TryGetProperty("maxLength", out var maxLengthEl) &&
                    maxLengthEl.TryGetInt32(out var maxLength) &&
                    rawValue.Length > maxLength)
                    errors.Add($"{field.Name}: Must be at most {maxLength} characters.");

                if (field.Type == FormFieldTypes.Number)
                {
                    if (double.TryParse(rawValue, out var numericValue))
                    {
                        if (validation.TryGetProperty("min", out var minEl) &&
                            minEl.TryGetDouble(out var min) &&
                            numericValue < min)
                            errors.Add($"{field.Name}: Must be at least {min}.");

                        if (validation.TryGetProperty("max", out var maxEl) &&
                            maxEl.TryGetDouble(out var max) &&
                            numericValue > max)
                            errors.Add($"{field.Name}: Must be at most {max}.");
                    }
                    else
                    {
                        errors.Add($"{field.Name}: Must be a valid number.");
                    }
                }
            }
            catch (JsonException)
            {
                // Invalid ValidationJson — skip validation
            }
        }

        return errors;
    }
}