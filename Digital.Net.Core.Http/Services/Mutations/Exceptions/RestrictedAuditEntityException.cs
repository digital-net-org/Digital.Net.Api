namespace Digital.Net.Core.Http.Services.Mutations.Exceptions;

public class RestrictedAuditEntityException(string entityType)
    : Exception($"Mutations of '{entityType}' are restricted to administrators.");