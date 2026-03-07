using Digital.Net.Authentication.Controllers;
using Digital.Net.Controllers.Controllers;
using Microsoft.AspNetCore.Builder;

namespace Digital.Net.Controllers;

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