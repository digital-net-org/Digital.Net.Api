using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Services.Crud.Exceptions;

public class EntityValidationException(string? message) :
    DigitalException(message ?? "Could not validate entity payload");