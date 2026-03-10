namespace Digital.Net.Api.Services.ApiKeys.Exceptions;

public class ExpiredAtInThePastException() : Exception("Expiration date cannot be in the past.");
