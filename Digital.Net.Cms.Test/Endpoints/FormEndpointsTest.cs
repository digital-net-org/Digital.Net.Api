using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Lib.Messages;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Cms.Test.Endpoints;

public class FormEndpointsTest
{
    [ClassDataSource<TestApplication>]
    public required TestApplication Application { get; init; }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var user = Application.CreateUser(new TestUserPayload { IsActive = true });
        var client = Application.CreateClient();
        await client.Login(user);
        return client;
    }


    [Test]
    public async Task CreateForm_ShouldCreateForm()
    {
        var client = await CreateAuthenticatedClientAsync();
        var payload = new FormPayload { Name = "ContactForm" };

        var response = await client.CreateForm(payload);
        var result = await response.Content.ReadFromJsonAsync<Result<Guid>>();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value).IsNotEqualTo(Guid.Empty);
    }

    [Test]
    public async Task GetFormById_ShouldReturnForm()
    {
        var client = await CreateAuthenticatedClientAsync();
        var form = Application.GetCmsContext().BuildTestForm("MyForm");

        var response = await client.GetFormById(form.Id);
        var result = await response.Content.ReadFromJsonAsync<Result<FormDto>>();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value!.Id).IsEqualTo(form.Id);
        await Assert.That(result.Value.Name).IsEqualTo(form.Name);
    }

    [Test]
    public async Task GetFormById_ShouldReturnNotFound_WhenNotExists()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetFormById(Guid.NewGuid());

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetForms_ShouldReturnPaginatedForms()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = Application.GetCmsContext();
        context.BuildTestForm();
        context.BuildTestForm();
        context.BuildTestForm();

        var response = await client.GetForms(new FormQuery { Size = 2, Index = 1 });
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(result.GetProperty("total").GetInt32()).IsGreaterThanOrEqualTo(3);
    }

    [Test]
    public async Task GetForms_ShouldFilterByName()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = Application.GetCmsContext();
        context.BuildTestForm(name: "FilterPrefix_A");
        context.BuildTestForm(name: "FilterPrefix_B");
        context.BuildTestForm(name: "OtherForm");

        var response = await client.GetForms(new FormQuery { Name = "FilterPrefix" });
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var values = result.GetProperty("value").EnumerateArray().ToList();
        await Assert.That(values.All(v => v.GetProperty("name").GetString()!.StartsWith("FilterPrefix"))).IsTrue();
    }

    [Test]
    public async Task GetForms_ShouldFilterByPublished()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = Application.GetCmsContext();
        context.BuildTestForm(published: true);
        context.BuildTestForm(published: false);

        var response = await client.GetForms(new FormQuery { Published = true });
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var values = result.GetProperty("value").EnumerateArray().ToList();
        await Assert.That(values.All(v => v.GetProperty("published").GetBoolean())).IsTrue();
    }

    [Test]
    public async Task GetForms_ShouldReturn401_WhenUnauthenticated()
    {
        var client = Application.CreateClient();

        var response = await client.GetForms();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task PatchForm_ShouldUpdateForm()
    {
        var client = await CreateAuthenticatedClientAsync();
        var form = Application.GetCmsContext().BuildTestForm();

        var patch = new[] { new { op = "replace", path = "/Name", value = "UpdatedFormName" } };
        var response = await client.PatchForm(form.Id, patch);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var getResponse = await client.GetFormById(form.Id);
        var result = await getResponse.Content.ReadFromJsonAsync<Result<FormDto>>();
        await Assert.That(result!.Value!.Name).IsEqualTo("UpdatedFormName");
    }

    [Test]
    public async Task DeleteForm_ShouldDeleteForm()
    {
        var client = await CreateAuthenticatedClientAsync();
        var form = Application.GetCmsContext().BuildTestForm();

        var response = await client.DeleteForm(form.Id);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var getResponse = await client.GetFormById(form.Id);
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteForm_ShouldReturnNotFound_WhenNotExists()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.DeleteForm(Guid.NewGuid());

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteForm_ShouldCascadeDeleteFieldsAndSubmissions()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm();
        context.BuildTestFormField(form.Id, name: "email");
        context.BuildTestFormSubmission(form.Id, valuesJson: "{\"email\":\"a@b.com\"}");

        var response = await client.DeleteForm(form.Id);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(context.FormFields.Any(f => f.FormId == form.Id)).IsFalse();
        await Assert.That(context.FormSubmissions.Any(s => s.FormId == form.Id)).IsFalse();
    }


    [Test]
    public async Task CreateFormField_ShouldCreateField()
    {
        var client = await CreateAuthenticatedClientAsync();
        var form = Application.GetCmsContext().BuildTestForm();
        var payload = new FormFieldPayload { Name = "email", Type = "email", Label = "Email Address" };

        var response = await client.CreateFormField(form.Id, payload);
        var result = await response.Content.ReadFromJsonAsync<Result<Guid>>();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value).IsNotEqualTo(Guid.Empty);
    }

    [Test]
    public async Task CreateFormField_ShouldRejectInvalidType()
    {
        var client = await CreateAuthenticatedClientAsync();
        var form = Application.GetCmsContext().BuildTestForm();
        var payload = new FormFieldPayload { Name = "field", Type = "invalid_type", Label = "Label" };

        var response = await client.CreateFormField(form.Id, payload);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateFormField_ShouldReturnNotFound_WhenFormDoesNotExist()
    {
        var client = await CreateAuthenticatedClientAsync();
        var payload = new FormFieldPayload { Name = "email", Type = "text", Label = "Email" };

        var response = await client.CreateFormField(Guid.NewGuid(), payload);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task CreateFormField_ShouldReturnConflict_WhenDuplicateName()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm();
        context.BuildTestFormField(form.Id, name: "email");
        var payload = new FormFieldPayload { Name = "email", Type = "email", Label = "Email" };

        var response = await client.CreateFormField(form.Id, payload);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task GetFormFields_ShouldReturnFieldsOrderedBySortOrder()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm();
        context.BuildTestFormField(form.Id, name: "second", sortOrder: 2);
        context.BuildTestFormField(form.Id, name: "first", sortOrder: 1);
        context.BuildTestFormField(form.Id, name: "third", sortOrder: 3);

        var response = await client.GetFormFields(form.Id);
        var result = await response.Content.ReadFromJsonAsync<Result<List<FormFieldDto>>>();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value!.Count).IsEqualTo(3);
        await Assert.That(result.Value[0].Name).IsEqualTo("first");
        await Assert.That(result.Value[1].Name).IsEqualTo("second");
        await Assert.That(result.Value[2].Name).IsEqualTo("third");
    }

    [Test]
    public async Task PatchFormField_ShouldUpdateField()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm();
        var field = context.BuildTestFormField(form.Id, name: "message");

        var patch = new[] { new { op = "replace", path = "/Label", value = "Your Message" } };
        var response = await client.PatchFormField(form.Id, field.Id, patch);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var getResponse = await client.GetFormFieldById(form.Id, field.Id);
        var result = await getResponse.Content.ReadFromJsonAsync<Result<FormFieldDto>>();
        await Assert.That(result!.Value!.Label).IsEqualTo("Your Message");
    }

    [Test]
    public async Task PatchFormField_ShouldRejectInvalidType()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm();
        var field = context.BuildTestFormField(form.Id, name: "field");

        var patch = new[] { new { op = "replace", path = "/Type", value = "invalid_type" } };
        var response = await client.PatchFormField(form.Id, field.Id, patch);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task PatchFormField_ShouldRejectDuplicateName()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm();
        context.BuildTestFormField(form.Id, name: "existing");
        var field = context.BuildTestFormField(form.Id, name: "other");

        var patch = new[] { new { op = "replace", path = "/Name", value = "existing" } };
        var response = await client.PatchFormField(form.Id, field.Id, patch);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task DeleteFormField_ShouldDeleteField()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm();
        var field = context.BuildTestFormField(form.Id, name: "toDelete");

        var response = await client.DeleteFormField(form.Id, field.Id);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var getResponse = await client.GetFormFieldById(form.Id, field.Id);
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }


    [Test]
    public async Task GetSubmissions_ShouldReturnPaginatedSubmissions()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm();
        context.BuildTestFormSubmission(form.Id, valuesJson: "{\"email\":\"a@b.com\"}");
        context.BuildTestFormSubmission(form.Id, valuesJson: "{\"email\":\"c@d.com\"}");

        var response = await client.GetFormSubmissions(new FormSubmissionQuery { FormId = form.Id });
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(result.GetProperty("total").GetInt32()).IsGreaterThanOrEqualTo(2);
    }

    [Test]
    public async Task GetSubmissions_ShouldFilterByFormId()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = Application.GetCmsContext();
        var form1 = context.BuildTestForm();
        var form2 = context.BuildTestForm();
        context.BuildTestFormSubmission(form1.Id);
        context.BuildTestFormSubmission(form2.Id);

        var response = await client.GetFormSubmissions(new FormSubmissionQuery { FormId = form1.Id });
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var values = result.GetProperty("value").EnumerateArray().ToList();
        await Assert.That(values.All(v => v.GetProperty("formId").GetGuid() == form1.Id)).IsTrue();
    }

    [Test]
    public async Task GetSubmissionById_ShouldReturnSubmission()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm();
        var submission = context.BuildTestFormSubmission(form.Id, valuesJson: "{\"name\":\"Test\"}");

        var response = await client.GetFormSubmission(submission.Id);
        var result = await response.Content.ReadFromJsonAsync<Result<FormSubmissionDto>>();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value!.Id).IsEqualTo(submission.Id);
        await Assert.That(result.Value.ValuesJson).IsEqualTo("{\"name\":\"Test\"}");
    }

    [Test]
    public async Task DeleteSubmission_ShouldDeleteSubmission()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm();
        var submission = context.BuildTestFormSubmission(form.Id);

        var response = await client.DeleteFormSubmission(submission.Id);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var getResponse = await client.GetFormSubmission(submission.Id);
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }


    [Test]
    public async Task GetFormDefinition_ShouldReturnPublishedFormWithFields()
    {
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm(published: true);
        context.BuildTestFormField(form.Id, name: "email", type: "email", label: "Email", sortOrder: 1);
        context.BuildTestFormField(form.Id, name: "message", type: "textarea", label: "Message", sortOrder: 2);

        var client = Application.CreateApplicationClient();
        var response = await client.GetFormDefinition(form.Id);
        var result = await response.Content.ReadFromJsonAsync<Result<FormDto>>();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value!.Id).IsEqualTo(form.Id);
        await Assert.That(result.Value.Fields.Count).IsEqualTo(2);
        await Assert.That(result.Value.Fields[0].Name).IsEqualTo("email");
        await Assert.That(result.Value.Fields[1].Name).IsEqualTo("message");
    }

    [Test]
    public async Task GetFormDefinition_ShouldReturn404_WhenUnpublished_ForApplicationAuth()
    {
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm(published: false);

        var client = Application.CreateApplicationClient();
        var response = await client.GetFormDefinition(form.Id);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetFormDefinition_ShouldReturnUnpublishedForm_ForJwtAuth()
    {
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm(published: false);

        var client = await CreateAuthenticatedClientAsync();
        var response = await client.GetFormDefinition(form.Id);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }


    private static FormSubmitPayload MakePayload(Dictionary<string, string?> values) =>
        new() { Values = values, SubmitterIp = "1.2.3.4", UserAgent = "TestAgent/1.0" };

    [Test]
    public async Task SubmitForm_ShouldPersistSubmission()
    {
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm(published: true);
        context.BuildTestFormField(form.Id, name: "email", type: "email", label: "Email");

        var client = Application.CreateApplicationClient();
        var response = await client.SubmitForm(form.Id, MakePayload(new() { ["email"] = "test@example.com" }));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(context.FormSubmissions.Any(s => s.FormId == form.Id)).IsTrue();
    }

    [Test]
    public async Task SubmitForm_ShouldAcceptWithoutIpAndUserAgent()
    {
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm(published: true);
        context.BuildTestFormField(form.Id, name: "name", type: "text", label: "Name");

        var client = Application.CreateApplicationClient();
        var payload = new FormSubmitPayload { Values = new() { ["name"] = "Test" } };
        var response = await client.SubmitForm(form.Id, payload);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task SubmitForm_ShouldReturn400_WhenRequiredFieldMissing()
    {
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm(published: true);
        context.BuildTestFormField(form.Id, name: "email", type: "email", label: "Email", required: true);

        var client = Application.CreateApplicationClient();
        var response = await client.SubmitForm(form.Id, MakePayload([]));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task SubmitForm_ShouldReturn400_WhenEmailFormatInvalid()
    {
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm(published: true);
        context.BuildTestFormField(form.Id, name: "email", type: "email", label: "Email");

        var client = Application.CreateApplicationClient();
        var response = await client.SubmitForm(form.Id, MakePayload(new() { ["email"] = "not-an-email" }));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task SubmitForm_ShouldReturn400_WhenCheckboxValueInvalid()
    {
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm(published: true);
        context.BuildTestFormField(form.Id, name: "agree", type: "checkbox", label: "Agree");

        var client = Application.CreateApplicationClient();
        var response = await client.SubmitForm(form.Id, MakePayload(new() { ["agree"] = "yes" }));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task SubmitForm_ShouldAcceptValidCheckboxValue()
    {
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm(published: true);
        context.BuildTestFormField(form.Id, name: "agree", type: "checkbox", label: "Agree");

        var client = Application.CreateApplicationClient();
        var response = await client.SubmitForm(form.Id, MakePayload(new() { ["agree"] = "true" }));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task SubmitForm_ShouldReturn400_WhenSelectOptionInvalid()
    {
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm(published: true);
        context.BuildTestFormField(form.Id, name: "color", type: "select", label: "Color",
            optionsJson: "[\"red\",\"green\",\"blue\"]");

        var client = Application.CreateApplicationClient();
        var response = await client.SubmitForm(form.Id, MakePayload(new() { ["color"] = "purple" }));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task SubmitForm_ShouldAcceptValidSelectOption()
    {
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm(published: true);
        context.BuildTestFormField(form.Id, name: "color", type: "select", label: "Color",
            optionsJson: "[\"red\",\"green\",\"blue\"]");

        var client = Application.CreateApplicationClient();
        var response = await client.SubmitForm(form.Id, MakePayload(new() { ["color"] = "red" }));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task SubmitForm_ShouldReturn400_WhenRadioOptionInvalid()
    {
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm(published: true);
        context.BuildTestFormField(form.Id, name: "plan", type: "radio", label: "Plan",
            optionsJson: "[\"free\",\"pro\"]");

        var client = Application.CreateApplicationClient();
        var response = await client.SubmitForm(form.Id, MakePayload(new() { ["plan"] = "enterprise" }));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task SubmitForm_ShouldReturn400_WhenMinLengthViolated()
    {
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm(published: true);
        context.BuildTestFormField(form.Id, name: "bio", type: "textarea", label: "Bio",
            validationJson: "{\"minLength\": 10}");

        var client = Application.CreateApplicationClient();
        var response = await client.SubmitForm(form.Id, MakePayload(new() { ["bio"] = "Short" }));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task SubmitForm_ShouldReturn400_WhenMaxLengthViolated()
    {
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm(published: true);
        context.BuildTestFormField(form.Id, name: "username", type: "text", label: "Username",
            validationJson: "{\"maxLength\": 5}");

        var client = Application.CreateApplicationClient();
        var response = await client.SubmitForm(form.Id, MakePayload(new() { ["username"] = "toolongvalue" }));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task SubmitForm_ShouldReturn400_WhenNumberBelowMin()
    {
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm(published: true);
        context.BuildTestFormField(form.Id, name: "age", type: "number", label: "Age",
            validationJson: "{\"min\": 18}");

        var client = Application.CreateApplicationClient();
        var response = await client.SubmitForm(form.Id, MakePayload(new() { ["age"] = "16" }));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task SubmitForm_ShouldReturn400_WhenNumberAboveMax()
    {
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm(published: true);
        context.BuildTestFormField(form.Id, name: "quantity", type: "number", label: "Quantity",
            validationJson: "{\"max\": 100}");

        var client = Application.CreateApplicationClient();
        var response = await client.SubmitForm(form.Id, MakePayload(new() { ["quantity"] = "999" }));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task SubmitForm_ShouldReturn404_WhenFormNotPublished()
    {
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm(published: false);

        var client = Application.CreateApplicationClient();
        var response = await client.SubmitForm(form.Id, MakePayload([]));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task SubmitForm_ShouldIgnoreMessageTypeFields()
    {
        var context = Application.GetCmsContext();
        var form = context.BuildTestForm(published: true);
        context.BuildTestFormField(form.Id, name: "info", type: "message", label: "Info text", required: true);

        var client = Application.CreateApplicationClient();
        var response = await client.SubmitForm(form.Id, MakePayload([]));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }
}
