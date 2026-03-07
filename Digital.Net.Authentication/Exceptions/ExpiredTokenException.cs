using Digital.Net.Core.Exceptions.types;

namespace Digital.Net.Authentication.Exceptions;

public class ExpiredTokenException() : DigitalException("Token is expired");