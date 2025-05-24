using Digital.Net.Api.Core.Exceptions;

namespace Digital.Net.Api.Services.Documents.Exceptions;

public class DocumentNotFoundException() : DigitalException("This document could not be found in database");