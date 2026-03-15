using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Services.Documents.Exceptions;

public class DocumentNotFoundException() : DigitalException("This document could not be found in database");