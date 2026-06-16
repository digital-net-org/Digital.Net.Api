using System.Threading.Tasks;
using Digital.Net.Tests.Core.Factories;
using Microsoft.EntityFrameworkCore;
using TUnit.Core.Interfaces;

namespace Digital.Net.Tests.Core;

public abstract class DbServiceTest<TContext> : UnitTest, IAsyncInitializer
    where TContext : DbContext
{
    [ClassDataSource<DatabaseFixture>]
    public required DatabaseFixture DbFixture { get; init; }

    protected TContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await OnInitializingAsync();
        Context = DbFixture.CreateContext<TContext>();
        await OnInitializedAsync();
    }

    protected virtual Task OnInitializingAsync() => Task.CompletedTask;

    protected virtual Task OnInitializedAsync() => Task.CompletedTask;
}