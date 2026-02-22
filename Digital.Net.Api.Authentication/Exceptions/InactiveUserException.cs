using Digital.Net.Api.Core.Exceptions.types;

namespace Digital.Net.Api.Authentication.Exceptions;

public class InactiveUserException() : DigitalException("User is inactive");