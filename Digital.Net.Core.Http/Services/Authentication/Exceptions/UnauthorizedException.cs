using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Http.Services.Authentication.Exceptions;

public class IpNotFound() : DigitalException("Ip address could not be found in the request headers");