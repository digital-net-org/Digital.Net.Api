using System.ComponentModel.DataAnnotations.Schema;
using Digital.Lib.Net.Authentication.Services.Authentication.Events;

namespace SafariDigital.Data.Models.Events;

[Table("EventAuthentication")]
public class EventAuthentication : AuthenticationEvent;