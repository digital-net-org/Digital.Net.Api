using Digital.Net.Core.Exceptions.types;

namespace Digital.Net.Api.Services.Users.Exceptions;

public class CannotDemoteAdminException() : DigitalException("Cannot remove administrator privileges from a user.");
