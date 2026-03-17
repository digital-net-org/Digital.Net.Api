using System;
using System.IO;
using Digital.Net.Cms.Context;
using Digital.Net.Cms.Models;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Documents;
using Digital.Net.Lib.Random;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Digital.Net.Tests.Core.Factories.Data;

public static class TestMediaDataFactory
{
    public static (byte[] Bytes, string FileName, string ContentType) GenerateTestImage(
        int width = 100,
        int height = 100
    )
    {
        using var image = new Image<Rgba32>(width, height);
        var ms = new MemoryStream();
        image.Save(ms, new PngEncoder());
        return (ms.ToArray(), "test.png", "image/png");
    }

    public static Media BuildTestMedia(
        this CmsContext cmsContext,
        DigitalContext digitalContext,
        string? name = null,
        bool published = false,
        string? storagePath = null,
        int imageWidth = 100,
        int imageHeight = 100
    )
    {
        var (bytes, _, _) = GenerateTestImage(imageWidth, imageHeight);
        var fileName = $"{Guid.NewGuid()}.png";

        var document = new Document
        {
            FileName = fileName,
            MimeType = "image/png",
            FileSize = bytes.Length
        };
        digitalContext.Documents.Add(document);
        digitalContext.SaveChanges();

        if (storagePath is not null)
        {
            var fullPath = Path.Combine(storagePath, fileName);
            var dir = Path.GetDirectoryName(fullPath);
            if (dir is not null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllBytes(fullPath, bytes);
        }

        var media = new Media
        {
            Name = name ?? Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 8),
            DocumentId = document.Id,
            Published = published
        };
        cmsContext.Media.Add(media);
        cmsContext.SaveChanges();

        return media;
    }

    public static Media BuildTestSvgMedia(
        this CmsContext cmsContext,
        DigitalContext digitalContext,
        string? name = null,
        bool published = false,
        string? storagePath = null
    )
    {
        var svgContent = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"100\" height=\"100\"><circle cx=\"50\" cy=\"50\" r=\"40\"/></svg>"u8;
        var fileName = $"{Guid.NewGuid()}.svg";

        var document = new Document
        {
            FileName = fileName,
            MimeType = "image/svg+xml",
            FileSize = svgContent.Length
        };
        digitalContext.Documents.Add(document);
        digitalContext.SaveChanges();

        if (storagePath is not null)
        {
            var fullPath = Path.Combine(storagePath, fileName);
            var dir = Path.GetDirectoryName(fullPath);
            if (dir is not null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllBytes(fullPath, svgContent.ToArray());
        }

        var media = new Media
        {
            Name = name ?? Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 8),
            DocumentId = document.Id,
            Published = published
        };
        cmsContext.Media.Add(media);
        cmsContext.SaveChanges();

        return media;
    }
}
