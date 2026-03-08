using Digital.Net.Core.Exceptions.types;

namespace Digital.Net.Api.Services.Documents.Exceptions;

public class TooHeavyException() : DigitalException("File size is too large.");