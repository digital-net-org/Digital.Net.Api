using Digital.Net.Api.Core.Exceptions.types;

namespace Digital.Net.Api.Services.Documents.Exceptions;

public class DocumentNotFoundException() : DigitalException("This document could not be found in database");