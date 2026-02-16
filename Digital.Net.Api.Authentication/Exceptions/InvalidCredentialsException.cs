using Digital.Net.Api.Core.Exceptions;

namespace Digital.Net.Api.Authentication.Exceptions;

public class InvalidCredentialsException() : DigitalException("Provided credentials are invalid.");
