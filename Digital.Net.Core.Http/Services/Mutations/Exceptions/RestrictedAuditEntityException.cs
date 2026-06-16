using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Http.Services.Mutations.Exceptions;

public class RestrictedAuditEntityException(string entityType)
    : DigitalException($"Mutations of '{entityType}' are restricted to administrators.");