using System;
using Digital.Net.Cms.Context;
using Digital.Net.Cms.Models;
using Digital.Net.Lib.Random;

namespace Digital.Net.Tests.Core.Factories.Data;

public static class TestSheetDataFactory
{
    public static Sheet BuildTestSheet(
        this CmsContext context,
        string? name = null,
        string type = "css",
        string? content = null,
        bool published = false
    )
    {
        var sheet = new Sheet
        {
            Name = name ?? Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 8),
            Type = type,
            Content = content ?? $"/* {Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 16)} */",
            Published = published
        };
        context.Sheets.Add(sheet);
        context.SaveChanges();
        return sheet;
    }

    public static PageSheet BuildTestPageSheet(
        this CmsContext context,
        Guid pageId,
        Guid sheetId,
        int loadOrder = 0
    )
    {
        var pageSheet = new PageSheet
        {
            PageId = pageId,
            SheetId = sheetId,
            LoadOrder = loadOrder
        };
        context.PageSheets.Add(pageSheet);
        context.SaveChanges();
        return pageSheet;
    }
}
