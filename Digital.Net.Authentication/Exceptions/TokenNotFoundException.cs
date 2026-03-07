using Digital.Net.Core.Exceptions.types;

namespace Digital.Net.Authentication.Exceptions;

public class TokenNotFoundException() : DigitalException("Token could not be found in the request headers");
