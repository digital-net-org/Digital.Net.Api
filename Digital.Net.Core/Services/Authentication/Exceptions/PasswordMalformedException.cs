using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Services.Authentication.Exceptions;

public class PasswordMalformedException() : DigitalException("Provided password does not meet requirements.");