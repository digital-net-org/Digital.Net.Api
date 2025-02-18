// using Digital.Lib.Net.Entities.Repositories;
// using Digital.Lib.Net.TestTools.Integration;
// using Digital.Pages.Api;
// using Digital.Pages.Api.Dto.Entities;
// using Digital.Pages.Api.Test.Utils.ApiCollections;
// using Digital.Pages.Api.Test.Utils.ApiCollections.Models;
// using Digital.Pages.Api.Test.Utils.Factories;
//
// namespace Digital.Pages.Api.Test.Digital.Pages.Api.Controllers.FrameController;
//
// public class CreateApiTest : IntegrationTest<Program, DigitalContext>
// {
//     private readonly UserFactory _userFactory;
//
//     public CreateApiTest(AppFactory<Program, DigitalContext> fixture) : base(fixture)
//     {
//         Repository<User> userRepository = new(GetContext());
//         _userFactory = new UserFactory(userRepository);
//     }
//
//     [Fact]
//     public async Task CreateFrame_CreateFrameInDB()
//     {
//         var (user, password) = _userFactory.CreateUser();
//         await BaseClient.Login(user.Login, password);
//         await BaseClient.CreateFrame(new FramePayload("{\"test\":\"test\"}", "TestFrame"));
//
//         var saved = GetContext().Frames.First();
//         Assert.Equal("{\"test\":\"test\"}", saved.Data);
//         _userFactory.Dispose();
//     }
//
//     [Fact]
//     public async Task PatchFrame_PatchFrameInDB()
//     {
//         var (user, password) = _userFactory.CreateUser();
//         await BaseClient.Login(user.Login, password);
//
//         await BaseClient.CreateFrame(new FramePayload("{\"test\":\"test\"}", "TestFrame"));
//         var saved = GetContext().Frames.First();
//
//         await BaseClient.PatchFrame(saved.Id, new PatchFramePayload { Data = "TestData2" });
//         saved = GetContext().Frames.First();
//
//         Assert.Equal("TestData2", saved.Data!);
//         _userFactory.Dispose();
//     }
// }