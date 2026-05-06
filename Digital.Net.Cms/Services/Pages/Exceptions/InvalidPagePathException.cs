using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Cms.Services.Pages.Exceptions;

public class InvalidPagePathException()
    : DigitalException("The provided path or slug could not be found or is not published.");