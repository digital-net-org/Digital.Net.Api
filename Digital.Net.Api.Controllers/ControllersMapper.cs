using Digital.Net.Api.Authentication.Controllers;
using Digital.Net.Api.Controllers.Controllers;
using Microsoft.AspNetCore.Builder;

namespace Digital.Net.Api.Controllers;

public static class ControllersMapper
{
    public static WebApplication UseDigitalEndpoints(this WebApplication app)
    {
        app
            .MapRootEndpoints()
            .MapAuthenticationEndpoints()
            .MapUserEndpoints()
            .MapAdministrationEndpoints()
            .MapPageEndpoints()
            .MapValidationEndpoints();
        return app;
    }
}