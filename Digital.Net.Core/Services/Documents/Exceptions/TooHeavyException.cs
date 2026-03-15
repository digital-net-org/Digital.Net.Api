using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Services.Documents.Exceptions;

public class TooHeavyException() : DigitalException("File size is too large.");