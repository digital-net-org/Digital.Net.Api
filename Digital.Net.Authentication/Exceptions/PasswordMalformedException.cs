using Digital.Net.Core.Exceptions.types;

namespace Digital.Net.Authentication.Exceptions;

public class PasswordMalformedException() : DigitalException("Provided password does not meet requirements.");