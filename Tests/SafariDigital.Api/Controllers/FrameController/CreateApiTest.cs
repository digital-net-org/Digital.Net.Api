using System.Net.Http.Json;
using Digital.Net.Entities.Models;
using Digital.Net.TestTools.Integration;
using SafariDigital.Api;
using SafariDigital.Data.Context;
using SafariDigital.Data.Models.Database.Users;
using SafariDigital.Data.Models.Dto.Frames;
using SafariDigital.Services.Frames.Models;
using Tests.Utils.ApiCollections;
using Tests.Utils.Factories;

namespace Tests.SafariDigital.Api.Controllers.FrameController;

public class CreateApiTest : IntegrationTest<Program, SafariDigitalContext>
{
    private readonly UserFactory _userFactory;

    public CreateApiTest(AppFactory<Program, SafariDigitalContext> fixture) : base(fixture)
    {
        SafariDigitalRepository<User> userRepository = new(GetContext());
        _userFactory = new UserFactory(userRepository);
    }

    [Fact]
    public async Task CreateFrame_CreateFrameInDB()
    {
        var (user, password) = _userFactory.CreateUser();
        await BaseClient.Login(user.Username, password);
        var response = await BaseClient.CreateFrame(new CreateFrameRequest("TestData", "TestFrame", null));
        var result = await (await BaseClient.GetAllFrames()).Content.ReadFromJsonAsync<QueryResult<FrameModel>>();
        var saved = GetContext().Frames.First();

        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal("TestData", result?.Value.First().Data);
        Assert.Equal(Convert.ToBase64String("TestData"u8.ToArray()), saved.Data);
        _userFactory.Dispose();
    }
}