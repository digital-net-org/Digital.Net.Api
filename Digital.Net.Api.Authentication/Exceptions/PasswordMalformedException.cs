using Digital.Net.Api.Core.Exceptions;

namespace Digital.Net.Api.Authentication.Exceptions;

public class PasswordMalformedException() : DigitalException("Provided password does not meet requirements.");