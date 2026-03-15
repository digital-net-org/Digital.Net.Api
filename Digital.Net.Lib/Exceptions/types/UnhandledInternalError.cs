namespace Digital.Net.Lib.Exceptions.types;

public class UnhandledInternalError()
    : DigitalException("An internal error occured during processing of request. This should not happen in production.");