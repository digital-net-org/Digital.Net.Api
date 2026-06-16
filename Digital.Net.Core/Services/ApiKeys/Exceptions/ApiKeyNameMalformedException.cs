using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Services.ApiKeys.Exceptions;

public class ApiKeyNameMalformedException() : DigitalException(
    "API key name is invalid. Only letters, numbers, spaces, hyphens and underscores are allowed (max 64 characters)."
);
