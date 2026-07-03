using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Http.Services.Pagination;

public class InvalidOrderByException(string orderBy) : DigitalException($"'{orderBy}' is not a sortable column");