using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Services.Authentication.Exceptions;

public class InvalidCredentialsException() : DigitalException("Provided credentials are invalid.");
