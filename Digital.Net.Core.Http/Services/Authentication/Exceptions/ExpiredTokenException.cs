using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Http.Services.Authentication.Exceptions;

public class ExpiredTokenException() : DigitalException("Token is expired");