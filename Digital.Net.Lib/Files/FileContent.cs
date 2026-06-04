namespace Digital.Net.Lib.Files;

/// <summary>
///     Neutral, transport-agnostic representation of a stored file's bytes and metadata.
///     HTTP adapters (e.g. Core.Http) turn this into a <c>FileResult</c>; the domain never depends on ASP.NET.
/// </summary>
public record FileContent(byte[] Bytes, string FileName, string? ContentType, DateTime LastModified);
