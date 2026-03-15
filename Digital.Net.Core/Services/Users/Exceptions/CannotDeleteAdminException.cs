using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Services.Users.Exceptions;

public class CannotDeleteAdminException() : DigitalException("Cannot delete a user with administrator privileges.");
