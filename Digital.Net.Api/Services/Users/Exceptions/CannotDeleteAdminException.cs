using Digital.Net.Core.Exceptions.types;

namespace Digital.Net.Api.Services.Users.Exceptions;

public class CannotDeleteAdminException() : DigitalException("Cannot delete a user with administrator privileges.");
