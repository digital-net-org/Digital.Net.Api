using Digital.Net.Api.Core.Exceptions.types;

namespace Digital.Net.Api.Authentication.Exceptions;

public class TooManyAttemptsException() : DigitalException("Too many login attempts");