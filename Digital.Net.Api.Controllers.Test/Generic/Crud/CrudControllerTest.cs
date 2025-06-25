using System.Text.Json;
using Digital.Net.Api.Controllers.Controllers.PageApi.Dto;
using Digital.Net.Api.Controllers.Generic.Crud;
using Digital.Net.Api.Controllers.Generic.Pagination;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Pages;
using Digital.Net.Api.Entities.Services;
using Digital.Net.Api.TestUtilities;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Digital.Net.Api.Controllers.Test.Generic.Crud;

public class CrudControllerTest : UnitTest
{
    private class TestCrudController(
        IEntityService<Page, DigitalContext> entityService,
        IEntityValidator<DigitalContext> entityValidator)
        : CrudController<Page, DigitalContext, PageDto, PagePayload>(entityService, entityValidator);

    private readonly CrudController<Page, DigitalContext, PageDto, PagePayload> _crudController;
    private readonly Mock<IEntityService<Page, DigitalContext>> _entityServiceMock = new();
    private readonly Mock<IEntityValidator<DigitalContext>> _entityValidatorMock = new();

    public CrudControllerTest()
    {
        _entityServiceMock
            .Setup(x => x.Get<PageDto>(It.IsAny<Guid>()))
            .Returns(new QueryResult<PageDto>());
        _entityServiceMock
            .Setup(x => x.Patch(It.IsAny<JsonPatchDocument<Page>>(), It.IsAny<Guid>()))
            .ReturnsAsync(new Result().AddError(new InvalidOperationException()));
        _crudController = new TestCrudController(_entityServiceMock.Object, _entityValidatorMock.Object);
    }

    [Fact]
    public async Task Patch_InvalidPayload_ShouldReturnBadRequest()
    {
        var jsonPatch = JsonDocument
            .Parse("[{ \"op\": \"replace\", \"path\": \"/name\", \"value\": \"newName\" }]")
            .RootElement;

        var result = await _crudController.Patch(Guid.NewGuid(), jsonPatch);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}