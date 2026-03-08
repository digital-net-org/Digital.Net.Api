using Digital.Net.Core.Exceptions.types;

namespace Digital.Net.Api.Services.Authentication.Exceptions;

public class TooManyAttemptsException() : DigitalException("Too many login attempts");