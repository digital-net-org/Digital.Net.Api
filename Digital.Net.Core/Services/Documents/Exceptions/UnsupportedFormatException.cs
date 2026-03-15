using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Services.Documents.Exceptions;

public class UnsupportedFormatException() : DigitalException("File format isn't supported");