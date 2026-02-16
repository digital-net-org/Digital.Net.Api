using Digital.Net.Api.Core.Exceptions;

namespace Digital.Net.Api.Authentication.Exceptions;

public class TooManyAttemptsException() : DigitalException("Too many login attempts");