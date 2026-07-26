namespace Digital.Net.Core.Http.Services.Authentication.Types;

/// <summary>
///     Distinguishes the two JWT flavours so a refresh token cannot be replayed as a bearer (and vice versa).
/// </summary>
public enum TokenType
{
    Access,
    Refresh
}
