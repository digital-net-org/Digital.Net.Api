using Digital.Net.Api.Core.Exceptions;

namespace Digital.Net.Api.Services.Authentication.Exceptions;

public class TokenNotFoundException() : DigitalException("Token could not be found in the request headers");
