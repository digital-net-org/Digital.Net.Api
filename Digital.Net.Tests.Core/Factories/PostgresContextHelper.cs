using System;
using Digital.Net.Lib.Entities.Context;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Tests.Core.Factories;

public static class PostgresContextHelper
{
    public static T CreateContext<T>(string connectionString) where T : DbContext
    {
        var options = new DbContextOptionsBuilder<T>()
            .UseDigitalNpgsql<T>(connectionString)
            .Options;

        return (T)Activator.CreateInstance(typeof(T), options)!
               ?? throw new InvalidOperationException($"Could not instantiate {typeof(T).Name}.");
    }
}