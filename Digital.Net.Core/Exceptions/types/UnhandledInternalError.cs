namespace Digital.Net.Core.Exceptions.types;

public class UnhandledInternalError()
    : DigitalException("An internal error occured during processing of request. This should not happen in production.");