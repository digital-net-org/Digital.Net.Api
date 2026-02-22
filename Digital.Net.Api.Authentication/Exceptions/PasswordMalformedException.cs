using Digital.Net.Api.Core.Exceptions.types;

namespace Digital.Net.Api.Authentication.Exceptions;

public class PasswordMalformedException() : DigitalException("Provided password does not meet requirements.");