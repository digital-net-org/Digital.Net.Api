using Digital.Net.Core.Exceptions.types;

namespace Digital.Net.Api.Services.Users.Exceptions;

public class CannotRevokeAdminException() : DigitalException("Cannot revoke a user with administrator privileges.");
