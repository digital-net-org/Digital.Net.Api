using System.ComponentModel.DataAnnotations.Schema;
using Digital.Lib.Net.Authentication.Services.Authentication.Events;

namespace Digital.Pages.Data.Models.Events;

[Table("EventAuthentication")]
public class EventAuthentication : AuthenticationEvent;