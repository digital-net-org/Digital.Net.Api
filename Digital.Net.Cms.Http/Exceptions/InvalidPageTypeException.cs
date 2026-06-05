using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Cms.Http.Exceptions;

public class InvalidPageTypeException() : DigitalException("The provided PageType does not match the requested Page.");