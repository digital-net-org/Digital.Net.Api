using Digital.Net.Core.Exceptions.types;

namespace Digital.Net.Api.Services.Authentication.Exceptions;

public class InvalidCredentialsException() : DigitalException("Provided credentials are invalid.");
