using System.Text.Json;
using Digital.Net.Api.Controllers.Generic.Pagination;
using Digital.Net.Api.Controllers.Test.TestUtilities.Context;
using Digital.Net.Api.Controllers.Test.TestUtilities.Controllers;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Services;
using Digital.Net.Api.TestUtilities;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Digital.Net.Api.Controllers.Test.Generic.Crud;

public class CrudControllerTest : UnitTest
{
    private readonly CrudControllerWithGuid _crudGuidController;

    private readonly CrudControllerWithId _crudIdController;
    private readonly Mock<IEntityService<TestGuidEntity, MvcTestContext>> _guidEntityServiceMock = new();
    private readonly Mock<IEntityService<TestIdEntity, MvcTestContext>> _idEntityServiceMock = new();
    private readonly Mock<IEntityValidator<MvcTestContext>> _entityValidatorMock = new();

    public CrudControllerTest()
    {
        _idEntityServiceMock
            .Setup(x => x.Get<TestIdEntityDto>(It.IsAny<int>()))
            .Returns(new QueryResult<TestIdEntityDto>());
        _guidEntityServiceMock
            .Setup(x => x.Get<TestGuidEntityDto>(It.IsAny<Guid>()))
            .Returns(new QueryResult<TestGuidEntityDto>());
        _guidEntityServiceMock
            .Setup(x => x.Patch(It.IsAny<JsonPatchDocument<TestGuidEntity>>(), It.IsAny<Guid>()))
            .ReturnsAsync(new Result().AddError(new InvalidOperationException()));

        _crudIdController = new CrudControllerWithId(_idEntityServiceMock.Object, _entityValidatorMock.Object);
        _crudGuidController = new CrudControllerWithGuid(_guidEntityServiceMock.Object, _entityValidatorMock.Object);
    }

    [Fact]
    public void GetById_AsInt_ShouldCallGetByWitchCorrectSignature()
    {
        const int id = 1;
        _crudIdController.GetById(1.ToString());
        _idEntityServiceMock.Verify(x => x.Get<TestIdEntityDto>(id), Times.Once);
    }

    [Fact]
    public void GetById_AsGuid_ShouldCallGetByWitchCorrectSignature()
    {
        var id = Guid.NewGuid();
        _crudGuidController.GetById(id.ToString());
        _guidEntityServiceMock.Verify(x => x.Get<TestGuidEntityDto>(id), Times.Once);
    }

    [Fact]
    public void GetById_InvalidId_ShouldReturnNotFound()
    {
        var result = _crudGuidController.GetById("invalidId");
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Patch_InvalidPayload_ShouldReturnBadRequest()
    {
        var jsonPatch = JsonDocument
            .Parse("[{ \"op\": \"replace\", \"path\": \"/name\", \"value\": \"newName\" }]")
            .RootElement;

        var result = await _crudGuidController.Patch(Guid.NewGuid().ToString(), jsonPatch);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}