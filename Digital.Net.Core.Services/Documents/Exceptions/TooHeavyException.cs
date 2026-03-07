using Digital.Net.Core.Exceptions.types;

namespace Digital.Net.Core.Services.Documents.Exceptions;

public class TooHeavyException() : DigitalException("File size is too large.");