using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Tests.Core.Factories;

public class DatabaseFixture
{
    [ClassDataSource<PostgresFixture>(Shared = SharedType.PerTestSession)]
    public required PostgresFixture Fixture { get; init; }

    public T CreateContext<T>() where T : DbContext => PostgresContextHelper.CreateContext<T>(Fixture.ConnectionString);

    public Task EnsureCreatedAsync<T>() where T : DbContext => Fixture.EnsureCreatedAsync<T>();
}