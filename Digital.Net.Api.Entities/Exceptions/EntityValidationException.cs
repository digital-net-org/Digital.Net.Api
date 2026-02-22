using Digital.Net.Api.Core.Exceptions.types;

namespace Digital.Net.Api.Entities.Exceptions;

public class EntityValidationException(string? message) :
    DigitalException(message ?? "Could not validate entity payload");