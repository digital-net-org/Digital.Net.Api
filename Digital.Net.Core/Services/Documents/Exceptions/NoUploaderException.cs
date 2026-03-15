using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Services.Documents.Exceptions;

public class NoUploaderException() : DigitalException("No user found in context, could not upload file.");