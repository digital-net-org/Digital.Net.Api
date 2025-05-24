using Digital.Net.Api.Controllers.Generic.Pagination;
using Digital.Net.Api.Controllers.Test.TestUtilities.Context;
using Digital.Net.Api.Entities.Repositories;

namespace Digital.Net.Api.Controllers.Test.TestUtilities.Controllers;

public class PaginationControllerWithId(IRepository<TestIdEntity, MvcTestContext> repository)
    : PaginationController<TestIdEntity, MvcTestContext, TestIdEntityDto, TestIdEntityQuery>(repository);