using System.ComponentModel.DataAnnotations.Schema;
using Digital.Lib.Net.Authentication.Models.Authorizations;

namespace SafariDigital.Data.Models.ApiTokens;

[Table("ApiToken")]
public class ApiToken : AuthorizationToken;