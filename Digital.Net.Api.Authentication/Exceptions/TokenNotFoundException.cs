using Digital.Net.Api.Core.Exceptions;

namespace Digital.Net.Api.Authentication.Exceptions;

public class TokenNotFoundException() : DigitalException("Token could not be found in the request headers");
