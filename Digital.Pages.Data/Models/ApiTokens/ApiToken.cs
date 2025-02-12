using System.ComponentModel.DataAnnotations.Schema;
using Digital.Lib.Net.Authentication.Models.Authorizations;

namespace Digital.Pages.Data.Models.ApiTokens;

[Table("ApiToken")]
public class ApiToken : AuthorizationToken;