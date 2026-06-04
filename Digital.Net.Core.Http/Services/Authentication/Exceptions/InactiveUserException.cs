using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Http.Services.Authentication.Exceptions;

public class InactiveUserException() : DigitalException("User is inactive");