using Digital.Net.Api.Core.Exceptions;

namespace Digital.Net.Api.Authentication.Exceptions;

public class InactiveUserException() : DigitalException("User is inactive");