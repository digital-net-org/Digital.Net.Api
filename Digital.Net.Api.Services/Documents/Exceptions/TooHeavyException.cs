using Digital.Net.Api.Core.Exceptions;

namespace Digital.Net.Api.Services.Documents.Exceptions;

public class TooHeavyException() : DigitalException("File size is too large.");