using Digital.Net.Core.Exceptions.types;

namespace Digital.Net.Core.Services.Documents.Exceptions;

public class NoUploaderException() : DigitalException("No user found in context, could not upload file.");