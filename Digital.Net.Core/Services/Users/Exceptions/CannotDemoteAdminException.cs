using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Services.Users.Exceptions;

public class CannotDemoteAdminException() : DigitalException("Cannot remove administrator privileges from a user.");
