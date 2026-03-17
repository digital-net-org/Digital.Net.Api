using System.Net;
using System.Net.Http.Json;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Core.Services.Pagination;
using Digital.Net.Lib.Messages;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Cms.Test.Endpoints;

public class SheetEndpointsTest
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

    // --- CRUD Tests ---

    [Test]
    public async Task CreateSheet_ShouldCreateSheet()
    {
        var client = await CreateAuthenticatedClientAsync();
        var payload = new SheetPayload { Name = "TestSheet", Type = "css", Content = "body { color: red; }" };

        var response = await client.CreateSheet(payload);
        var result = await response.Content.ReadFromJsonAsync<Result<Guid>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value).IsNotEqualTo(Guid.Empty);
    }

    [Test]
    public async Task CreateSheet_ShouldRejectInvalidType()
    {
        var client = await CreateAuthenticatedClientAsync();
        var payload = new SheetPayload { Name = "InvalidSheet", Type = "html", Content = "<div></div>" };

        var response = await client.CreateSheet(payload);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetSheetById_ShouldReturnSheet()
    {
        var client = await CreateAuthenticatedClientAsync();
        var sheet = Application.GetCmsContext().BuildTestSheet("FindById");

        var response = await client.GetSheetById(sheet.Id);
        var result = await response.Content.ReadFromJsonAsync<Result<SheetDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value!.Id).IsEqualTo(sheet.Id);
        await Assert.That(result.Value.Name).IsEqualTo(sheet.Name);
    }

    [Test]
    public async Task GetSheetById_ShouldReturnNotFound_WhenSheetDoesNotExist()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetSheetById(Guid.NewGuid());

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetSheets_ShouldReturnPaginatedSheets()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = Application.GetCmsContext();
        context.BuildTestSheet();
        context.BuildTestSheet();
        context.BuildTestSheet();

        var response = await client.GetSheets(new SheetQuery { Size = 2, Index = 1 });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<SheetDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Size).IsEqualTo(2);
        await Assert.That(result.Index).IsEqualTo(1);
        await Assert.That(result.Total).IsGreaterThanOrEqualTo(3);
    }

    [Test]
    public async Task GetSheets_ShouldFilterByType()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = Application.GetCmsContext();
        context.BuildTestSheet(type: "css");
        context.BuildTestSheet(type: "js");

        var response = await client.GetSheets(new SheetQuery { Type = "js" });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<SheetDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value.All(s => s.Type == "js")).IsTrue();
    }

    [Test]
    public async Task PatchSheet_ShouldUpdateSheet()
    {
        var client = await CreateAuthenticatedClientAsync();
        var sheet = Application.GetCmsContext().BuildTestSheet();

        var patch = new[] { new { op = "replace", path = "/Name", value = "UpdatedSheetName" } };
        var response = await client.PatchSheet(sheet.Id, patch);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var getResponse = await client.GetSheetById(sheet.Id);
        var result = await getResponse.Content.ReadFromJsonAsync<Result<SheetDto>>();
        await Assert.That(result!.Value!.Name).IsEqualTo("UpdatedSheetName");
    }

    [Test]
    public async Task DeleteSheet_ShouldDeleteSheet()
    {
        var client = await CreateAuthenticatedClientAsync();
        var sheet = Application.GetCmsContext().BuildTestSheet();

        var response = await client.DeleteSheet(sheet.Id);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var getResponse = await client.GetSheetById(sheet.Id);
        await Assert.That(getResponse.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteSheet_ShouldReturnNotFound_WhenSheetDoesNotExist()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.DeleteSheet(Guid.NewGuid());

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }

    // --- Association Tests ---

    [Test]
    public async Task AssociateSheet_ShouldAssociateSheetToPage()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = Application.GetCmsContext();
        var page = context.BuildTestPage();
        var sheet = context.BuildTestSheet(published: true);

        var payload = new PageSheetPayload { SheetId = sheet.Id, LoadOrder = 1 };
        var response = await client.AssociateSheet(page.Id, payload);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task AssociateSheet_ShouldReturnConflict_WhenAlreadyAssociated()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = Application.GetCmsContext();
        var page = context.BuildTestPage();
        var sheet = context.BuildTestSheet();
        context.BuildTestPageSheet(page.Id, sheet.Id);

        var payload = new PageSheetPayload { SheetId = sheet.Id, LoadOrder = 0 };
        var response = await client.AssociateSheet(page.Id, payload);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task DissociateSheet_ShouldRemoveAssociation()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = Application.GetCmsContext();
        var page = context.BuildTestPage();
        var sheet = context.BuildTestSheet();
        context.BuildTestPageSheet(page.Id, sheet.Id);

        var response = await client.DissociateSheet(page.Id, sheet.Id);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task DissociateSheet_ShouldReturnNotFound_WhenNotAssociated()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.DissociateSheet(Guid.NewGuid(), Guid.NewGuid());

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }

    // --- Resource Serving Tests ---

    [Test]
    public async Task GetResource_ShouldServeCssContent()
    {
        var context = Application.GetCmsContext();
        var sheet = context.BuildTestSheet(type: "css", content: "body { color: red; }", published: true);
        var client = Application.CreateApplicationClient();

        var response = await client.GetResource(sheet.Id, "css");

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(response.Content.Headers.ContentType!.MediaType).IsEqualTo("text/css");
        var content = await response.Content.ReadAsStringAsync();
        await Assert.That(content).IsEqualTo("body { color: red; }");
    }

    [Test]
    public async Task GetResource_ShouldServeJsContent()
    {
        var context = Application.GetCmsContext();
        var sheet = context.BuildTestSheet(type: "js", content: "console.log('hello');", published: true);
        var client = Application.CreateApplicationClient();

        var response = await client.GetResource(sheet.Id, "js");

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(response.Content.Headers.ContentType!.MediaType).IsEqualTo("application/javascript");
        var content = await response.Content.ReadAsStringAsync();
        await Assert.That(content).IsEqualTo("console.log('hello');");
    }

    [Test]
    public async Task GetResource_ShouldReturnNotFound_WhenUnpublished()
    {
        var context = Application.GetCmsContext();
        var sheet = context.BuildTestSheet(published: false);
        var client = Application.CreateApplicationClient();

        var response = await client.GetResource(sheet.Id, "css");

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }

    // --- Page Sheets Listing Tests ---

    [Test]
    public async Task GetPageSheets_ShouldReturnPublishedSheetsOrderedByLoadOrder()
    {
        var context = Application.GetCmsContext();
        var page = context.BuildTestPage(published: true);
        var sheet1 = context.BuildTestSheet(name: "Second", type: "css", published: true);
        var sheet2 = context.BuildTestSheet(name: "First", type: "js", published: true);
        var unpublished = context.BuildTestSheet(name: "Hidden", published: false);
        context.BuildTestPageSheet(page.Id, sheet1.Id, loadOrder: 2);
        context.BuildTestPageSheet(page.Id, sheet2.Id, loadOrder: 1);
        context.BuildTestPageSheet(page.Id, unpublished.Id, loadOrder: 0);

        var client = Application.CreateApplicationClient();
        var response = await client.GetPageSheets(page.Id);
        var result = await response.Content.ReadFromJsonAsync<List<PageSheetDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Count).IsEqualTo(2);
        await Assert.That(result[0].Name).IsEqualTo("First");
        await Assert.That(result[1].Name).IsEqualTo("Second");
    }
}
