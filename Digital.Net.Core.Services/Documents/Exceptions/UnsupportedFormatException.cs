using Digital.Net.Core.Exceptions.types;

namespace Digital.Net.Core.Services.Documents.Exceptions;

public class UnsupportedFormatException() : DigitalException("File format isn't supported");