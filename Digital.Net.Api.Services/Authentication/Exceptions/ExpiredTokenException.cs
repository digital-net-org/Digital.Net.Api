using Digital.Net.Api.Core.Exceptions;

namespace Digital.Net.Api.Services.Authentication.Exceptions;

public class ExpiredTokenException() : DigitalException("Token is expired");