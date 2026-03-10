namespace Digital.Net.Api.Services.ApiKeys.Exceptions;

public class ApiKeyNameMalformedException() : Exception("API key name is invalid. Only letters, numbers, spaces, hyphens and underscores are allowed (max 64 characters).");
