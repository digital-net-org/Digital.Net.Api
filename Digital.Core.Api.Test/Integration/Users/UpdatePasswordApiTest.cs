// using System.Net;
// using Digital.Core.Api.Test.TestUtilities;
// using Digital.Lib.Net.Entities.Repositories;
// using Digital.Lib.Net.TestTools.Integration;
//
// namespace Digital.Core.Api.Test.Integration.UserController;
//
// public class UpdatePasswordApiTest : IntegrationTest<Program, DigitalContext>
// {
//     private readonly UserFactory _userFactory;
//
//     public UpdatePasswordApiTest(AppFactory<Program, DigitalContext> fixture) : base(fixture)
//     {
//         Repository<User> userRepository = new(GetContext());
//         _userFactory = new UserFactory(userRepository);
//     }
//
//     [Fact]
//     public async Task UpdatePassword_ShouldReturnOk()
//     {
//         var (user, password) = _userFactory.CreateUser();
//         var res = await AuthenticationCollection.Login(BaseClient, (string)user.Login, (string)password);
//         var response = await BaseClient.UpdatePassword(user.Id, password, "1newPassword*");
//         Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//     }
// }