using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Services.Authentication.Exceptions;

public class TokenNotFoundException() : DigitalException("Token could not be found in the request headers");
