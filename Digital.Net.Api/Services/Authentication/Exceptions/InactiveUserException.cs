using Digital.Net.Core.Exceptions.types;

namespace Digital.Net.Api.Services.Authentication.Exceptions;

public class InactiveUserException() : DigitalException("User is inactive");