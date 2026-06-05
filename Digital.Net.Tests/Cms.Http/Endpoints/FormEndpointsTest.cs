using System.Net;
using System.Net.Http.Json;
using Digital.Net.Cms.Http.Dto;
using Digital.Net.Cms.Models.Forms;
using Digital.Net.Core.Http.Services.Pagination;
using Digital.Net.Lib.Messages;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Tests.Cms.Http.Endpoints;

public class FormEndpointsTest
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

    private static FormFieldPayload BuildFieldPayload(
        string? type = null,
        string? name = null,
        string? label = null
    ) => new()
    {
        Name = name ?? "field-" + Guid.NewGuid().ToString("N")[..6],
        Type = type ?? FormFieldTypes.Text,
        Label = label ?? "Label",
        Required = false,
        SortOrder = 0
    };

    [Test]
    public async Task CreateForm_ShouldPersistPath_WhenPathIsValid()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.CreateForm(new FormCreatePayload
        {
            Name = "Contact-" + Guid.NewGuid().ToString("N")[..6],
            Path = "/contact-" + Guid.NewGuid().ToString("N")[..6]
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task CreateForm_ShouldRejectInvalidPath()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.CreateForm(new FormCreatePayload
        {
            Name = "BadPath-" + Guid.NewGuid().ToString("N")[..6],
            Path = "not-a-valid-path"
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateForm_ShouldAcceptNullPath()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.CreateForm(new FormCreatePayload
        {
            Name = "Nopath-" + Guid.NewGuid().ToString("N")[..6]
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var ctx = ApplicationFixture.GetCmsContext();
        var created = await ctx.Forms.OrderByDescending(f => f.CreatedAt).FirstAsync();
        await Assert.That(created.Path).IsNull();
    }

    [Test]
    public async Task GetForms_ShouldFilterByPath_UsingStrictEquality()
    {
        var client = await CreateAuthenticatedClientAsync();
        var ctx = ApplicationFixture.GetCmsContext();
        var slug = Guid.NewGuid().ToString("N")[..6];
        var sharedPath = $"/exact-{slug}";
        var matchedA = ctx.BuildTestForm(name: $"matched-a-{slug}");
        matchedA.Path = sharedPath;
        var matchedB = ctx.BuildTestForm(name: $"matched-b-{slug}");
        matchedB.Path = sharedPath;
        var nonMatched = ctx.BuildTestForm(name: $"prefix-{slug}");
        nonMatched.Path = $"/exact-{slug}-other";
        await ctx.SaveChangesAsync();

        var response = await client.GetForms(new FormQuery { Path = sharedPath, Size = 50, Index = 1 });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<FormDto>>();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value.All(f => f.Path == sharedPath)).IsTrue();
        await Assert.That(result.Value.Any(f => f.Id == matchedA.Id)).IsTrue();
        await Assert.That(result.Value.Any(f => f.Id == matchedB.Id)).IsTrue();
        await Assert.That(result.Value.Any(f => f.Id == nonMatched.Id)).IsFalse();
    }

    [Test]
    public async Task GetForm_ShouldIncludeFieldsAndPath()
    {
        var client = await CreateAuthenticatedClientAsync();
        var ctx = ApplicationFixture.GetCmsContext();
        var form = ctx.BuildTestForm();
        form.Path = "/with-fields-" + Guid.NewGuid().ToString("N")[..6];
        await ctx.SaveChangesAsync();
        ctx.BuildTestFormField(formId: form.Id, type: FormFieldTypes.Text, sortOrder: 1);
        ctx.BuildTestFormField(formId: form.Id, type: FormFieldTypes.Email, sortOrder: 0);

        var response = await client.GetFormById(form.Id);
        var result = await response.Content.ReadFromJsonAsync<Result<FormDto>>();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value!.Path).IsEqualTo(form.Path);
        await Assert.That(result.Value.Fields.Count).IsEqualTo(2);
        await Assert.That(result.Value.Fields[0].SortOrder).IsEqualTo(0);
    }

    [Test]
    public async Task CreateFormField_ShouldPersist_WhenTypeIsAllowed()
    {
        var client = await CreateAuthenticatedClientAsync();
        var ctx = ApplicationFixture.GetCmsContext();
        var form = ctx.BuildTestForm();

        var response = await client.CreateFormField(form.Id, BuildFieldPayload(type: FormFieldTypes.Email));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var count = await ctx.FormFields.AsNoTracking().CountAsync(f => f.FormId == form.Id);
        await Assert.That(count).IsEqualTo(1);
    }

    [Test]
    public async Task CreateFormField_ShouldReject_WhenTypeIsUnknown()
    {
        var client = await CreateAuthenticatedClientAsync();
        var ctx = ApplicationFixture.GetCmsContext();
        var form = ctx.BuildTestForm();

        var response = await client.CreateFormField(form.Id, BuildFieldPayload(type: "not-a-type"));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateFormField_ShouldReturnNotFound_WhenFormDoesNotExist()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.CreateFormField(Guid.NewGuid(), BuildFieldPayload());

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task PatchFormField_ShouldReturnNotFound_WhenFieldBelongsToAnotherForm()
    {
        var client = await CreateAuthenticatedClientAsync();
        var ctx = ApplicationFixture.GetCmsContext();
        var formA = ctx.BuildTestForm();
        var formB = ctx.BuildTestForm();
        var fieldOfB = ctx.BuildTestFormField(formId: formB.Id);

        var response = await client.PatchFormField(
            formA.Id,
            fieldOfB.Id,
            new[] { new { op = "replace", path = "/label", value = "hijacked" } }
        );

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        var unchanged = await ctx.FormFields.AsNoTracking().FirstAsync(f => f.Id == fieldOfB.Id);
        await Assert.That(unchanged.Label).IsNotEqualTo("hijacked");
    }

    [Test]
    public async Task PatchFormField_ShouldReject_WhenPatchTargetsFormId()
    {
        var client = await CreateAuthenticatedClientAsync();
        var ctx = ApplicationFixture.GetCmsContext();
        var formA = ctx.BuildTestForm();
        var formB = ctx.BuildTestForm();
        var fieldOfA = ctx.BuildTestFormField(formId: formA.Id);

        var response = await client.PatchFormField(
            formA.Id,
            fieldOfA.Id,
            new[] { new { op = "replace", path = "/formId", value = formB.Id } }
        );

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        var unchanged = await ctx.FormFields.AsNoTracking().FirstAsync(f => f.Id == fieldOfA.Id);
        await Assert.That(unchanged.FormId).IsEqualTo(formA.Id);
    }

    [Test]
    public async Task PatchFormField_ShouldUpdateLabel_WhenValid()
    {
        var client = await CreateAuthenticatedClientAsync();
        var ctx = ApplicationFixture.GetCmsContext();
        var form = ctx.BuildTestForm();
        var field = ctx.BuildTestFormField(formId: form.Id, label: "old");

        var response = await client.PatchFormField(
            form.Id,
            field.Id,
            new[] { new { op = "replace", path = "/label", value = "renamed" } }
        );

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var refreshed = await ctx.FormFields.AsNoTracking().FirstAsync(f => f.Id == field.Id);
        await Assert.That(refreshed.Label).IsEqualTo("renamed");
    }

    [Test]
    public async Task DeleteFormField_ShouldReturnNotFound_WhenFieldBelongsToAnotherForm()
    {
        var client = await CreateAuthenticatedClientAsync();
        var ctx = ApplicationFixture.GetCmsContext();
        var formA = ctx.BuildTestForm();
        var formB = ctx.BuildTestForm();
        var fieldOfB = ctx.BuildTestFormField(formId: formB.Id);

        var response = await client.DeleteFormField(formA.Id, fieldOfB.Id);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        var stillExists = await ctx.FormFields.AsNoTracking().AnyAsync(f => f.Id == fieldOfB.Id);
        await Assert.That(stillExists).IsTrue();
    }

    [Test]
    public async Task DeleteForm_ShouldCascade_OnFields()
    {
        var client = await CreateAuthenticatedClientAsync();
        var ctx = ApplicationFixture.GetCmsContext();
        var form = ctx.BuildTestForm();
        var field1 = ctx.BuildTestFormField(formId: form.Id);
        var field2 = ctx.BuildTestFormField(formId: form.Id);

        var response = await client.DeleteForm(form.Id);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var fieldsLeft = await ctx.FormFields.AsNoTracking()
            .Where(f => f.Id == field1.Id || f.Id == field2.Id)
            .CountAsync();
        await Assert.That(fieldsLeft).IsEqualTo(0);
    }

    [Test]
    public async Task CreateFormField_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var client = ApplicationFixture.CreateClient();
        var ctx = ApplicationFixture.GetCmsContext();
        var form = ctx.BuildTestForm();

        var response = await client.CreateFormField(form.Id, BuildFieldPayload());

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetFormSubmissions_ShouldReturnUnauthorized_WhenOnlyApplicationKeyIsUsed()
    {
        var applicationClient = ApplicationFixture.CreateApplicationClient();

        var response = await applicationClient.GetFormSubmissions();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }
}
