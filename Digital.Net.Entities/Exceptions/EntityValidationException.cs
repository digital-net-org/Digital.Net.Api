using Digital.Net.Core.Exceptions.types;

namespace Digital.Net.Entities.Exceptions;

public class EntityValidationException(string? message) :
    DigitalException(message ?? "Could not validate entity payload");