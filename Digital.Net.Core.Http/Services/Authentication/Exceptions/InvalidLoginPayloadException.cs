using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Http.Services.Authentication.Exceptions;

public class InvalidLoginPayloadException() : DigitalException("Password or login cannot be processed");