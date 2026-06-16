using Digital.Net.Lib.Exceptions.types;

namespace Digital.Net.Core.Services.ApiKeys.Exceptions;

public class ExpiredAtInThePastException() : DigitalException("Expiration date cannot be in the past.");
