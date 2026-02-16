using Digital.Net.Api.Core.Exceptions;

namespace Digital.Net.Api.Authentication.Exceptions;

public class ExpiredTokenException() : DigitalException("Token is expired");