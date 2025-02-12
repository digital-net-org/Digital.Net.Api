using Digital.Lib.Net.Entities.Repositories;
using Digital.Lib.Net.TestTools.Integration;
using SafariDigital.Api;
using SafariDigital.Api.Dto.Entities;
using SafariDigital.Data.Context;
using SafariDigital.Data.Models.Users;
using Tests.Utils.ApiCollections;
using Tests.Utils.ApiCollections.Models;
using Tests.Utils.Factories;

namespace Tests.SafariDigital.Api.Controllers.FrameController;

public class CreateApiTest : IntegrationTest<Program, SafariDigitalContext>
{
    private readonly UserFactory _userFactory;

    public CreateApiTest(AppFactory<Program, SafariDigitalContext> fixture) : base(fixture)
    {
        Repository<User> userRepository = new(GetContext());
        _userFactory = new UserFactory(userRepository);
    }

    [Fact]
    public async Task CreateFrame_CreateFrameInDB()
    {
        var (user, password) = _userFactory.CreateUser();
        await BaseClient.Login(user.Login, password);
        await BaseClient.CreateFrame(new FramePayload("{\"test\":\"test\"}", "TestFrame"));

        var saved = GetContext().Frames.First();
        Assert.Equal("{\"test\":\"test\"}", saved.Data);
        _userFactory.Dispose();
    }

    [Fact]
    public async Task PatchFrame_PatchFrameInDB()
    {
        var (user, password) = _userFactory.CreateUser();
        await BaseClient.Login(user.Login, password);

        await BaseClient.CreateFrame(new FramePayload("{\"test\":\"test\"}", "TestFrame"));
        var saved = GetContext().Frames.First();

        await BaseClient.PatchFrame(saved.Id, new PatchFramePayload { Data = "TestData2" });
        saved = GetContext().Frames.First();

        Assert.Equal("TestData2", saved.Data!);
        _userFactory.Dispose();
    }
}