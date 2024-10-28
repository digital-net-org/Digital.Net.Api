using System.Text;

namespace SafariDigital.Data.Models.Database.Frames;

public static class FrameExtensions
{
    public static void EncodeData(this Frame frame) =>
        frame.Data = frame.Data is not null ? Convert.ToBase64String(Encoding.UTF8.GetBytes(frame.Data)) : null;

    public static string? GetDecodedData(this Frame frame) =>
        frame.Data is not null ? Encoding.UTF8.GetString(Convert.FromBase64String(frame.Data)) : null;
}