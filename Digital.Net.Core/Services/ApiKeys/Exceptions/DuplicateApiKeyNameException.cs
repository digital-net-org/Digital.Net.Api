using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Services.ApiKeys.Exceptions;

public class DuplicateApiKeyNameException() : DigitalException("An API key with this name already exists.");
