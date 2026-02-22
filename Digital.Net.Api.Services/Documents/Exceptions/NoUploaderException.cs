using Digital.Net.Api.Core.Exceptions.types;

namespace Digital.Net.Api.Services.Documents.Exceptions;

public class NoUploaderException() : DigitalException("No user found in context, could not upload file.");