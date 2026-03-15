using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Services.Users.Exceptions;

public class CannotRevokeAdminException() : DigitalException("Cannot revoke a user with administrator privileges.");
