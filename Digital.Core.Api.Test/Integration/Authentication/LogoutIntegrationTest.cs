// using System.Net;
// using Digital.Lib.Net.Entities.Repositories;
// using Digital.Lib.Net.TestTools.Integration;
// using InternalTestProgram;
// using InternalTestProgram.Extensions;
// using InternalTestProgram.Factories;
// using InternalTestProgram.Models;
//
// namespace Digital.Lib.Net.Authentication.Test.Jwt;
//
// public class LogoutIntegrationTest : IntegrationTest<Program, TestContext>
// {
//     private readonly TestUserFactory _testUserFactory;
//     private readonly IRepository<ApiToken> _apiTokenRepository;
//
//     public LogoutIntegrationTest(AppFactory<Program, TestContext> fixture) : base(fixture)
//     {
//         _testUserFactory = new TestUserFactory(new Repository<TestUser>(GetContext()));
//         _apiTokenRepository = new Repository<ApiToken>(GetContext());
//     }
//
//     [Fact]
//     public async Task Logout_ShouldLogoutClient()
//     {
//         var (user, password) = _testUserFactory.Create();
//         await BaseClient.Login(user.Login, password);
//         Assert.True(await _apiTokenRepository.CountAsync(x => x.ApiUserId == user.Id) == 1);
//
//         var response = await BaseClient.Logout();
//         Assert.True(await _apiTokenRepository.CountAsync(x => x.ApiUserId == user.Id) == 0);
//         Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
//     }
//
//     [Fact]
//     public async Task LogoutAll_ShouldLogoutAllClients()
//     {
//         var (user, password) = _testUserFactory.Create();
//         await BaseClient.Login(user.Login, password);
//         await BaseClient.Login(user.Login, password);
//         Assert.True(await _apiTokenRepository.CountAsync(x => x.ApiUserId == user.Id) == 2);
//
//         var response = await BaseClient.LogoutAll();
//         Assert.True(await _apiTokenRepository.CountAsync(x => x.ApiUserId == user.Id) == 0);
//         Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
//     }
// }