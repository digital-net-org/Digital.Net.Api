using Digital.Net.Api.Core.Exceptions;

namespace Digital.Net.Api.Services.Documents.Exceptions;

public class UnsupportedFormatException() : DigitalException("File format isn't supported");