using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Services.Authentication.Exceptions;

public class TooManyAttemptsException() : DigitalException("Too many login attempts");