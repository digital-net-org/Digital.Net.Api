using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Services.ApiKeys.Exceptions;

public class MaxApiKeysReachedException() : DigitalException("Maximum number of API keys reached.");
