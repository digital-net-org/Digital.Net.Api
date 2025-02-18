// using System.Net;
// using Digital.Lib.Net.Core.Extensions.StringUtilities;
// using Digital.Lib.Net.Entities.Repositories;
// using Digital.Lib.Net.Http.HttpClient.Extensions;
// using Digital.Lib.Net.TestTools.Integration;
// using InternalTestProgram;
// using InternalTestProgram.Extensions;
// using InternalTestProgram.Factories;
// using InternalTestProgram.Models;
//
// namespace Digital.Lib.Net.Authentication.Test.Jwt;
//
// public class RefreshIntegrationTest : IntegrationTest<Program, TestContext>
// {
//     private readonly TestUserFactory _testUserFactory;
//     private readonly IRepository<ApiToken> _apiTokenRepository;
//
//     public RefreshIntegrationTest(AppFactory<Program, TestContext> fixture) : base(fixture)
//     {
//         _testUserFactory = new TestUserFactory(new Repository<TestUser>(GetContext()));
//         _apiTokenRepository = new Repository<ApiToken>(GetContext());
//     }
//
//     [Fact]
//     public async Task RefreshTokens_WithValidRefreshToken_ShouldReturnToken()
//     {
//         var (user, password) = _testUserFactory.Create();
//         await BaseClient.Login(user.Login, password);
//         var response = await BaseClient.RefreshTokens();
//
//         Assert.True((await response.Content.ReadAsStringAsync()).IsJsonWebToken());
//         Assert.True(response.Headers.TryGetCookie("Cookie")?.IsJsonWebToken());
//     }
//
//     [Fact]
//     public async Task RefreshTokens_WithInvalidToken_ShouldReturnUnauthorized()
//     {
//         var (user, password) = _testUserFactory.Create();
//         await BaseClient.Login(user.Login, password);
//         await BaseClient.Logout();
//         var response = await BaseClient.RefreshTokens();
//         Assert.True(response.StatusCode == HttpStatusCode.Unauthorized);
//     }
// }