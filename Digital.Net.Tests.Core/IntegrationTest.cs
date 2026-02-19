using System.Threading.Tasks;
using Digital.Net.Tests.Core.Factories;

namespace Digital.Net.Tests.Core;

public abstract class IntegrationTest
{
    public static ApplicationFactory Application = null!;

    [Before(Class)]
    public static void Setup() => Application = new ApplicationFactory();

    [After(Class)]
    public static async Task TeardownAsync()
    {
        try
        {
            await Application.DisposeAsync();
        }
        catch
        {
            // Ignore
        }
    }
}