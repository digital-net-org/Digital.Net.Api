using Digital.Net.Api.Controllers.Generic.Crud;
using Digital.Net.Api.Controllers.Test.TestUtilities.Context;
using Digital.Net.Api.Entities.Services;

namespace Digital.Net.Api.Controllers.Test.TestUtilities.Controllers;

public class CrudControllerWithGuid(
    IEntityService<TestGuidEntity, MvcTestContext> entityService,
    IEntityValidator<MvcTestContext> entityValidator
) : CrudController<TestGuidEntity, MvcTestContext, TestGuidEntityDto, TestGuidEntityPayload>(
    entityService,
    entityValidator
);