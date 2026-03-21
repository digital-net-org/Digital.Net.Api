using System.Linq.Expressions;
using System.Text.Json;
using Digital.Net.Cms.Context;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Cms.Endpoints.Events;
using Digital.Net.Cms.Models;
using Digital.Net.Core.RateLimiter.Limiters;
using Digital.Net.Core.Services.Auditing;
using Digital.Net.Core.Services.Authentication;
using Digital.Net.Core.Services.Authentication.Filters;
using Digital.Net.Core.Services.Authentication.Options;
using Digital.Net.Core.Services.Authentication.Types;
using Digital.Net.Core.Services.Crud;
using Digital.Net.Core.Services.Crud.Extensions;
using Digital.Net.Core.Services.Pagination.Extensions;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Lib.Formatters;
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
    private static readonly string[] ValidFieldTypes =
        ["text", "email", "textarea", "number", "select", "radio", "checkbox", "message"];

    public static IEndpointRouteBuilder MapCmsFormEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("cms/forms")
            .WithTags("CMS - Forms")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapCrudSchema<CmsContext, Form>("");

        controller
            .MapCrudGet<Form, FormDto>("");

        controller
            .MapPaginationGet<CmsContext, Form, FormDto, FormQuery>("", PaginationFilter);

        controller
            .MapPost("", CreateForm)
            .WithSummary("Create")
            .WithDescription("Creates a new form.");

        controller
            .MapPatch("{id:guid}", UpdateForm)
            .WithSummary("Patch")
            .WithDescription("Updates a form by its ID.");

        controller
            .MapDelete("{id:guid}", DeleteForm)
            .WithSummary("Delete")
            .WithDescription("Deletes a form by its ID.");

        // FormField endpoints
        var fields = app
            .MapGroup("cms/forms/{formId:guid}/fields")
            .WithTags("CMS - Forms")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        fields
            .MapGet("", GetFormFields)
            .WithSummary("GetFormFields")
            .WithDescription("Returns all fields of a form ordered by SortOrder.");

        fields
            .MapGet("{id:guid}", GetFormFieldById)
            .WithSummary("GetFormFieldById")
            .WithDescription("Returns a form field by its ID.");

        fields
            .MapPost("", CreateFormField)
            .WithSummary("CreateFormField")
            .WithDescription("Creates a new field for the specified form.");

        fields
            .MapPatch("{id:guid}", UpdateFormField)
            .WithSummary("PatchFormField")
            .WithDescription("Updates a form field by its ID.");

        fields
            .MapDelete("{id:guid}", DeleteFormField)
            .WithSummary("DeleteFormField")
            .WithDescription("Deletes a form field by its ID.");

        // Submission admin endpoints
        var submissions = app
            .MapGroup("cms/forms/{id:guid}/submissions")
            .WithTags("CMS - Forms")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        submissions
            .MapGet("", GetSubmissions)
            .WithSummary("GetSubmissions")
            .WithDescription("Returns a paginated list of submissions for a form.");

        submissions
            .MapGet("{submissionId:guid}", GetSubmissionById)
            .WithSummary("GetSubmissionById")
            .WithDescription("Returns a submission by its ID.");

        submissions
            .MapDelete("{submissionId:guid}", DeleteSubmission)
            .WithSummary("DeleteSubmission")
            .WithDescription("Deletes a submission by its ID.");

        // Application-facing endpoints
        app
            .MapGroup("cms/forms")
            .WithTags("CMS - Forms")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .MapGet("{id:guid}/definition", GetFormDefinition)
            .WithSummary("GetFormDefinition")
            .WithDescription(
                "Returns the full definition of a published form. "
                + "Unpublished forms return 404 for Application auth but remain accessible via JWT and API Key.")
            .RequireAuthentication(AuthorizeType.Application | AuthorizeType.Jwt | AuthorizeType.ApiKey);

        app
            .MapGroup("cms/forms")
            .WithTags("CMS - Forms")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .MapPost("{id:guid}/submit", SubmitForm)
            .WithSummary("SubmitForm")
            .WithDescription("Submits values for a published form. Validates all fields server-side.")
            .RequireAuthentication(AuthorizeType.Application);

        return app;
    }

    // --- Form CRUD ---

    private static async Task<IResult> CreateForm(
        [FromBody] FormPayload payload,
        ICrudService<Form> crudService,
        IAuditService auditService,
        IUserContextService userContextService
    )
    {
        var entity = new Form { Name = payload.Name, Description = payload.Description };
        if (payload.SubmitLabel is not null)
            entity.SubmitLabel = payload.SubmitLabel;
        var result = await crudService.Create(entity);
        await auditService.RegisterEventAsync(
            CmsEvents.CreateForm,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );
        return result.HasError ? Results.BadRequest(result) : Results.Ok(result);
    }

    private static async Task<IResult> UpdateForm(
        Guid id,
        [FromBody] JsonElement patch,
        ICrudService<Form> crudService,
        IAuditService auditService,
        IUserContextService userContextService
    )
    {
        var result = await crudService.Patch(patch.GetPatchDocument<Form>(), id);
        await auditService.RegisterEventAsync(
            CmsEvents.UpdateForm,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );
        return result.HasError ? Results.BadRequest(result) : Results.Ok(result);
    }

    private static async Task<IResult> DeleteForm(
        Guid id,
        ICrudService<Form> crudService,
        IAuditService auditService,
        IUserContextService userContextService
    )
    {
        var result = await crudService.Delete(id);
        await auditService.RegisterEventAsync(
            CmsEvents.DeleteForm,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );
        return result.HasError ? Results.NotFound(result) : Results.Ok(result);
    }

    // --- FormField CRUD ---

    private static async Task<Results<Ok<Result<List<FormFieldDto>>>, NotFound>> GetFormFields(
        Guid formId,
        CmsContext context
    )
    {
        var form = await context.Forms.FindAsync(formId);
        if (form is null)
            return TypedResults.NotFound();

        var fields = await context.FormFields
            .Where(f => f.FormId == formId)
            .OrderBy(f => f.SortOrder)
            .Select(f => new FormFieldDto(f))
            .ToListAsync();

        return TypedResults.Ok(new Result<List<FormFieldDto>>(fields));
    }

    private static async Task<Results<Ok<Result<FormFieldDto>>, NotFound>> GetFormFieldById(
        Guid formId,
        Guid id,
        CmsContext context
    )
    {
        var field = await context.FormFields.FirstOrDefaultAsync(f => f.Id == id && f.FormId == formId);
        if (field is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(new Result<FormFieldDto>(new FormFieldDto(field)));
    }

    private static async Task<IResult> CreateFormField(
        Guid formId,
        [FromBody] FormFieldPayload payload,
        ICrudService<FormField> crudService,
        IAuditService auditService,
        IUserContextService userContextService,
        CmsContext context
    )
    {
        if (!ValidFieldTypes.Contains(payload.Type))
            return Results.BadRequest($"Invalid type '{payload.Type}'. Must be one of: {string.Join(", ", ValidFieldTypes)}.");

        var form = await context.Forms.FindAsync(formId);
        if (form is null)
            return Results.NotFound("Form not found.");

        var duplicate = await context.FormFields.AnyAsync(f => f.FormId == formId && f.Name == payload.Name);
        if (duplicate)
            return Results.Conflict($"A field named '{payload.Name}' already exists in this form.");

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
        await auditService.RegisterEventAsync(
            CmsEvents.CreateFormField,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );
        return result.HasError ? Results.BadRequest(result) : Results.Ok(result);
    }

    private static async Task<IResult> UpdateFormField(
        Guid formId,
        Guid id,
        [FromBody] JsonElement patch,
        ICrudService<FormField> crudService,
        IAuditService auditService,
        IUserContextService userContextService,
        CmsContext context
    )
    {
        var field = await context.FormFields.FirstOrDefaultAsync(f => f.Id == id && f.FormId == formId);
        if (field is null)
            return Results.NotFound();

        var result = await crudService.Patch(patch.GetPatchDocument<FormField>(), id);
        await auditService.RegisterEventAsync(
            CmsEvents.UpdateFormField,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );
        return result.HasError ? Results.BadRequest(result) : Results.Ok(result);
    }

    private static async Task<IResult> DeleteFormField(
        Guid formId,
        Guid id,
        ICrudService<FormField> crudService,
        IAuditService auditService,
        IUserContextService userContextService,
        CmsContext context
    )
    {
        var field = await context.FormFields.FirstOrDefaultAsync(f => f.Id == id && f.FormId == formId);
        if (field is null)
            return Results.NotFound();

        var result = await crudService.Delete(id);
        await auditService.RegisterEventAsync(
            CmsEvents.DeleteFormField,
            result.HasError ? EventState.Failed : EventState.Success,
            result,
            userContextService.GetUserId()
        );
        return result.HasError ? Results.BadRequest(result) : Results.Ok(result);
    }

    // --- Submissions (admin) ---

    private static async Task<Results<Ok<Result<List<FormSubmissionDto>>>, NotFound>> GetSubmissions(
        Guid id,
        CmsContext context
    )
    {
        var form = await context.Forms.FindAsync(id);
        if (form is null)
            return TypedResults.NotFound();

        var submissions = await context.FormSubmissions
            .Where(s => s.FormId == id)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new FormSubmissionDto(s))
            .ToListAsync();

        return TypedResults.Ok(new Result<List<FormSubmissionDto>>(submissions));
    }

    private static async Task<Results<Ok<Result<FormSubmissionDto>>, NotFound>> GetSubmissionById(
        Guid id,
        Guid submissionId,
        CmsContext context
    )
    {
        var submission = await context.FormSubmissions
            .FirstOrDefaultAsync(s => s.Id == submissionId && s.FormId == id);
        if (submission is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(new Result<FormSubmissionDto>(new FormSubmissionDto(submission)));
    }

    private static async Task<IResult> DeleteSubmission(
        Guid id,
        Guid submissionId,
        CmsContext context
    )
    {
        var submission = await context.FormSubmissions
            .FirstOrDefaultAsync(s => s.Id == submissionId && s.FormId == id);
        if (submission is null)
            return Results.NotFound();

        context.FormSubmissions.Remove(submission);
        await context.SaveChangesAsync();
        return Results.Ok();
    }

    // --- Application-facing ---

    private static async Task<Results<Ok<Result<FormDto>>, NotFound>> GetFormDefinition(
        Guid id,
        CmsContext context,
        HttpContext httpContext
    )
    {
        var form = await context.Forms
            .Include(f => f.Fields)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (form is null)
            return TypedResults.NotFound();

        var authResult = httpContext.Items[AuthenticationStaticOptions.ApiContextAuthorizationKey] as AuthorizationResult;
        var isApplicationAuth = authResult?.UserId == Guid.Empty;
        if (isApplicationAuth && !form.Published)
            return TypedResults.NotFound();

        return TypedResults.Ok(new Result<FormDto>(new FormDto(form)));
    }

    private static async Task<IResult> SubmitForm(
        Guid id,
        [FromBody] FormSubmitPayload payload,
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

    // --- Helpers ---

    private static List<string> ValidateSubmission(
        List<FormField> fields,
        Dictionary<string, string?> values
    )
    {
        var errors = new List<string>();

        foreach (var field in fields.Where(f => f.Type != "message"))
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

            if (field.Type == "email" && !System.Text.RegularExpressions.Regex.IsMatch(rawValue, RegularExpressions.EmailPattern))
            {
                errors.Add($"{field.Name}: Invalid email address.");
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

                if (validation.TryGetProperty("pattern", out var patternEl))
                {
                    var pattern = patternEl.GetString();
                    if (pattern is not null && !System.Text.RegularExpressions.Regex.IsMatch(rawValue, pattern))
                        errors.Add($"{field.Name}: Value does not match the required pattern.");
                }

                if (field.Type == "number")
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

    private static Expression<Func<Form, bool>> PaginationFilter(
        Expression<Func<Form, bool>> predicate,
        FormQuery query
    )
    {
        if (!string.IsNullOrEmpty(query.Name))
            predicate = predicate.Add(x => x.Name.StartsWith(query.Name));
        if (query.Published.HasValue)
            predicate = predicate.Add(x => x.Published == query.Published);
        return predicate;
    }
}
