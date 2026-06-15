using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Lib.Entities.Exceptions;

public class EntityValidationException(string? message) :
    DigitalException(message ?? "Could not validate entity payload");