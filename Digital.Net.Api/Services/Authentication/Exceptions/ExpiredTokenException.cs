using Digital.Net.Core.Exceptions.types;

namespace Digital.Net.Api.Services.Authentication.Exceptions;

public class ExpiredTokenException() : DigitalException("Token is expired");