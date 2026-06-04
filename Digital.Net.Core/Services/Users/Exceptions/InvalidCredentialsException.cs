using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Services.Users.Exceptions;

public class InvalidCredentialsException() : DigitalException("Provided credentials are invalid.");