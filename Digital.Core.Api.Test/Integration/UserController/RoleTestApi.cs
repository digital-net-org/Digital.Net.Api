// using System.Net;
// using Digital.Lib.Net.Entities.Context;
// using Digital.Lib.Net.Entities.Models.Users;
// using Digital.Lib.Net.Entities.Repositories;
// using Digital.Lib.Net.TestTools.Integration;
// using Digital.Pages.Api;
// using Digital.Pages.Api.Test.Utils.ApiCollections;
// using Digital.Pages.Api.Test.Utils.Factories;
//
// namespace Digital.Pages.Api.Test.Digital.Pages.Api.Controllers.AuthenticationController;
//
// public class RoleTestApi : IntegrationTest<Program, DigitalContext>
// {
//     private readonly UserFactory _userFactory;
//
//
//     public RoleTestApi(AppFactory<Program, DigitalContext> fixture) : base(fixture)
//     {
//         Repository<User, DigitalContext> userRepository = new(GetContext());
//         _userFactory = new UserFactory(userRepository);
//     }
//
//     [Fact]
//     public async Task Visitor_ShouldOnlyAccessPublicEndpoints()
//     {
//         var unauthorizedUserResponse = await BaseClient.TestUserAuthorization();
//         var unauthorizedAdminResponse = await BaseClient.TestAdminAuthorization();
//         var unauthorizedSuperAdminResponse = await BaseClient.TestSuperAdminAuthorization();
//         Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedUserResponse.StatusCode);
//         Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedAdminResponse.StatusCode);
//         Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedSuperAdminResponse.StatusCode);
//     }
//
//     [Fact]
//     public async Task User_ShouldAccessUserEndpoints()
//     {
//         var (user, password) = _userFactory.CreateUser();
//         await BaseClient.Login(user.Login, password);
//
//         var okUserResponse = await BaseClient.TestUserAuthorization();
//         var unauthorizedAdminResponse = await BaseClient.TestAdminAuthorization();
//         var unauthorizedSuperAdminResponse = await BaseClient.TestSuperAdminAuthorization();
//
//         Assert.Equal(HttpStatusCode.NoContent, okUserResponse.StatusCode);
//         Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedAdminResponse.StatusCode);
//         Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedSuperAdminResponse.StatusCode);
//     }
//
//     [Fact]
//     public async Task Admin_ShouldAccessAdminEndpoints()
//     {
//         var (admin, password) = _userFactory.CreateUser(new UserPayload { Role = UserRole.Admin });
//         await BaseClient.Login(admin.Login, password);
//
//         var okUserResponse = await BaseClient.TestUserAuthorization();
//         var okAdminResponse = await BaseClient.TestAdminAuthorization();
//         var unauthorizedSuperAdminResponse = await BaseClient.TestSuperAdminAuthorization();
//
//         Assert.Equal(HttpStatusCode.NoContent, okUserResponse.StatusCode);
//         Assert.Equal(HttpStatusCode.NoContent, okAdminResponse.StatusCode);
//         Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedSuperAdminResponse.StatusCode);
//     }
//
//     [Fact]
//     public async Task SuperAdmin_ShouldAccessSuperAdminEndpoints()
//     {
//         var (superAdmin, password) = _userFactory.CreateUser(new UserPayload { Role = UserRole.SuperAdmin });
//         await BaseClient.Login(superAdmin.Login, password);
//
//         var okUserResponse = await BaseClient.TestUserAuthorization();
//         var okAdminResponse = await BaseClient.TestAdminAuthorization();
//         var okSuperAdminResponse = await BaseClient.TestSuperAdminAuthorization();
//
//         Assert.Equal(HttpStatusCode.NoContent, okUserResponse.StatusCode);
//         Assert.Equal(HttpStatusCode.NoContent, okAdminResponse.StatusCode);
//         Assert.Equal(HttpStatusCode.NoContent, okSuperAdminResponse.StatusCode);
//     }
// }