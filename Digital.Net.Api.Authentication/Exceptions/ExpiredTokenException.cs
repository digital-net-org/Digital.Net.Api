using Digital.Net.Api.Core.Exceptions.types;

namespace Digital.Net.Api.Authentication.Exceptions;

public class ExpiredTokenException() : DigitalException("Token is expired");