using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SafariDigital.Database.Context;

namespace Tests.Core.Integration;

public sealed class SqliteDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteDatabase()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        var contextOptions = new DbContextOptionsBuilder<SafariDigitalContext>().UseSqlite(_connection).Options;
        _connection.Open();

        Context = new SafariDigitalContext(contextOptions);
        Context.Database.EnsureCreated();
    }

    public SafariDigitalContext Context { get; }

    public void Dispose() => _connection.Close();
}