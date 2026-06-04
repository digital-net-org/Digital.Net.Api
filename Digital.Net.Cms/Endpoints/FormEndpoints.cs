using System.Linq.Expressions;
using System.Text.Json;
using System.Text.RegularExpressions;
using Digital.Net.Cms.Context;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Cms.Endpoints.Events;
using Digital.Net.Cms.Models.Forms;
using Digital.Net.Core.Accessors;
using Digital.Net.Core.Entities.Exceptions;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.Http.RateLimiters;
using Digital.Net.Core.Http.Services.Authentication.Filters;
using Digital.Net.Core.Http.Services.Crud;
using Digital.Net.Core.Http.Services.Pagination.Extensions;
using Digital.Net.Core.Services.Auditing;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Lib.Messages;
using Digital.Net.Lib.Predicates;
using Digital.Net.Lib.String;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Endpoints;

public static class FormEndpoints
{
    public static IEndpointRouteBuilder MapCmsFormEndpoints(this IEndpointRouteBuilder app)
    {
        var form = app
            .MapGroup("cms/forms")
            .WithTags("CMS.Forms")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        form.MapCrudSchema<CmsContext, Form>();
        form.MapCrudGet<CmsContext, Form, FormDto>();
        form.MapPaginationGet<CmsContext, Form, FormDto, FormQuery>(filter: PaginationFilter);
        form.MapCrudPost<CmsContext, Form, FormCreatePayload>(eventType: CmsEvents.CreateForm);
        form.MapCrudDelete<CmsContext, Form>(eventType: CmsEvents.DeleteForm);
        form.MapCrudPatch<CmsContext, Form>(eventType: CmsEvents.UpdateForm);

        form.MapCrudSchema<CmsContext, FormField>("fields");

        form
            .MapPost("{formId:guid}/fields", CreateFormField)
            .WithSummary("CreateFormField")
            .WithDescription("Creates a new field attached to the form identified by formId.");

        form
            .MapPatch("{formId:guid}/fields/{fieldId:guid}", PatchFormField)
            .WithSummary("PatchFormField")
            .WithDescription("Applies a JSON Patch to a field owned by the form. Returns 404 if the field does not belong to the form.");

        form
            .MapDelete("{formId:guid}/fields/{fieldId:guid}", DeleteFormField)
            .WithSummary("DeleteFormField")
            .WithDescription("Deletes a field owned by the form. Returns 404 if the field does not belong to the form.");


        var submissions = app
            .MapGroup("cms/forms/submissions")
            .WithTags("CMS.FormsSubmissions")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        submissions.MapCrudGet<CmsContext, FormSubmission, FormSubmissionDto>();
        submissions.MapPaginationGet<CmsContext, FormSubmission, FormSubmissionDto, FormSubmissionQuery>(
            filter: SubmissionPaginationFilter
        );
        submissions.MapCrudDelete<CmsContext, FormSubmission>(eventType: CmsEvents.DeleteFormSubmission);

        var publicApi = app
            .MapGroup("cms/forms")
            .WithTags("CMS.FormsPublic")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Application | AuthorizeType.Jwt | AuthorizeType.ApiKey);

        publicApi
            .MapGet("{id:guid}/definition", GetFormDefinition)
            .WithSummary("GetFormDefinition")
            .WithDescription("Returns the full definition of a published form.");

        publicApi
            .MapPost("{id:guid}/submit", SubmitForm)
            .WithSummary("SubmitForm")
            .WithDescription("Submits values for a published form. Validates all fields server-side.")
            .RequireAuthentication(AuthorizeType.Application | AuthorizeType.Jwt | AuthorizeType.ApiKey);

        return app;
    }
    
    private static async Task<Results<Ok<Result<FormDto>>, NotFound>> GetFormDefinition(
        Guid id,
        CmsContext context,
        HttpContext httpContext
    )
    {
        var form = await context.Forms
            .Include(f => f.Fields)
            .FirstOrDefaultAsync(f => f.Id == id && f.Published == true);
        return form is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(new Result<FormDto>(new FormDto(form)));
    }

    private static async Task<IResult> SubmitForm(
        Guid id,
        [FromBody]
        FormSubmitPayload payload,
        CmsContext context,
        IAuditService auditService
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

        var submission = new FormSubmission
        {
            FormId = id,
            ValuesJson = JsonSerializer.Serialize(payload.Values),
            SubmitterIp = payload.SubmitterIp,
            UserAgent = payload.UserAgent
        };
        context.FormSubmissions.Add(submission);
        await context.SaveChangesAsync();

        await auditService.RegisterEventAsync(
            CmsEvents.FormSubmission,
            EventState.Success,
            new Result(),
            null
        );

        return Results.Ok(new Result());
    }

    private static List<string> ValidateSubmission(
        List<FormField> fields,
        Dictionary<string, string?> values
    )
    {
        var errors = new List<string>();

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

            if (field.Type == FormFieldTypes.Email &&
                !Regex.IsMatch(rawValue, RegularExpressions.EmailPattern))
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

    private static async Task<Results<Ok<Result<Guid>>, BadRequest<Result<Guid>>, NotFound<Result<Guid>>, InternalServerError<Result<Guid>>>>
        CreateFormField(
            Guid formId,
            [FromBody]
            FormFieldPayload payload,
            CmsContext context,
            CrudService<CmsContext, FormField> crudService,
            IAuditService auditService,
            IUserAccessor userContextService
        )
    {
        var formExists = await context.Forms.AsNoTracking().AnyAsync(f => f.Id == formId);
        if (!formExists)
            return TypedResults.NotFound(new Result<Guid>().AddError(new ResourceNotFoundException()));

        var entity = new FormField
        {
            FormId = formId,
            Name = payload.Name,
            Type = payload.Type,
            Label = payload.Label,
            Placeholder = payload.Placeholder,
            DefaultValue = payload.DefaultValue,
            Required = payload.Required,
            SortOrder = payload.SortOrder,
            ValidationJson = payload.ValidationJson,
            OptionsJson = payload.OptionsJson
        };

        var result = await crudService.Create(entity);
        var isBadRequest = result.HasErrorOfType<EntityValidationException>();
        if (result.HasError && !isBadRequest)
            return TypedResults.InternalServerError(result);

        await auditService.RegisterEventAsync(
            CmsEvents.CreateFormField,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );

        return result.HasError
            ? TypedResults.BadRequest(result)
            : TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<Result>, BadRequest<Result>, NotFound<Result>, InternalServerError<Result>>>
        PatchFormField(
            Guid formId,
            Guid fieldId,
            [FromBody]
            JsonElement patch,
            CmsContext context,
            CrudService<CmsContext, FormField> crudService,
            IAuditService auditService,
            IUserAccessor userContextService,
            CancellationToken ct
        )
    {
        var field = await context.FormFields.AsNoTracking().FirstOrDefaultAsync(f => f.Id == fieldId, ct);
        if (field is null || field.FormId != formId)
            return TypedResults.NotFound(new Result().AddError(new ResourceNotFoundException()));

        if (PatchTargetsFormId(patch))
            return TypedResults.BadRequest(
                new Result().AddError(new EntityValidationException("FormId cannot be patched."))
            );

        var result = await crudService.Patch(patch, fieldId, ct);
        var isBadRequest = result.HasErrorOfType<EntityValidationException>() ||
                           result.HasErrorOfType<InvalidOperationException>();

        if (result.HasErrorOfType<ResourceNotFoundException>())
            return TypedResults.NotFound(result);
        if (result.HasError && !isBadRequest)
            return TypedResults.InternalServerError(result);

        await auditService.RegisterEventAsync(
            CmsEvents.UpdateFormField,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );

        return result.HasError
            ? TypedResults.BadRequest(result)
            : TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<Result>, NotFound<Result>, InternalServerError<Result>>>
        DeleteFormField(
            Guid formId,
            Guid fieldId,
            CmsContext context,
            CrudService<CmsContext, FormField> crudService,
            IAuditService auditService,
            IUserAccessor userContextService
        )
    {
        var field = await context.FormFields.AsNoTracking().FirstOrDefaultAsync(f => f.Id == fieldId);
        if (field is null || field.FormId != formId)
            return TypedResults.NotFound(new Result().AddError(new ResourceNotFoundException()));

        var result = await crudService.Delete(fieldId);
        if (result.HasErrorOfType<ResourceNotFoundException>())
            return TypedResults.NotFound(result);
        if (result.HasError)
            return TypedResults.InternalServerError(result);

        await auditService.RegisterEventAsync(
            CmsEvents.DeleteFormField,
            EventState.Success,
            result,
            userContextService.GetUserId()
        );

        return TypedResults.Ok(result);
    }

    private static bool PatchTargetsFormId(JsonElement patch)
    {
        if (patch.ValueKind != JsonValueKind.Array)
            return false;
        foreach (var operation in patch.EnumerateArray())
        {
            if (operation.ValueKind != JsonValueKind.Object)
                continue;
            if (!operation.TryGetProperty("path", out var pathEl) || pathEl.ValueKind != JsonValueKind.String)
                continue;
            var path = pathEl.GetString();
            if (string.Equals(path, "/formId", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(path, "/FormId", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static Expression<Func<Form, bool>> PaginationFilter(
        Expression<Func<Form, bool>> predicate,
        FormQuery query
    )
    {
        if (!string.IsNullOrEmpty(query.Name))
            predicate = predicate.Add(x => x.Name.StartsWith(query.Name));
        if (query.Published.HasValue)
            predicate = predicate.Add(x => x.Published == query.Published);
        if (!string.IsNullOrEmpty(query.Path))
            predicate = predicate.Add(x => x.Path == query.Path);
        return predicate;
    }

    private static Expression<Func<FormSubmission, bool>> SubmissionPaginationFilter(
        Expression<Func<FormSubmission, bool>> predicate,
        FormSubmissionQuery query
    )
    {
        if (query.FormId.HasValue)
            predicate = predicate.Add(x => x.FormId == query.FormId);
        return predicate;
    }
}