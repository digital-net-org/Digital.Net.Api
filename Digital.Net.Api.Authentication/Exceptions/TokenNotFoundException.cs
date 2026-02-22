using Digital.Net.Api.Core.Exceptions.types;

namespace Digital.Net.Api.Authentication.Exceptions;

public class TokenNotFoundException() : DigitalException("Token could not be found in the request headers");
