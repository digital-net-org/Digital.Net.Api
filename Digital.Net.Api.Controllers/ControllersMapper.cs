using Digital.Net.Api.Controllers.Controllers;
using Microsoft.AspNetCore.Builder;

namespace Digital.Net.Api.Controllers;

public static class ControllersMapper
{
    public static WebApplication UseDigitalEndpoints(this WebApplication app)
    {
        app
            .MapRootEndpoints()
            .MapAdministrationEndpoints()
            .MapUserEndpoints()
            .MapValidationEndpoints();
        return app;
    }
}