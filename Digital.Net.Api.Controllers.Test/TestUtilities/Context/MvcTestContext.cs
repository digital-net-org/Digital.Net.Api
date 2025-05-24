using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Controllers.Test.TestUtilities.Context;

public class MvcTestContext(DbContextOptions<MvcTestContext> options) : DbContext(options)
{
    public DbSet<TestIdEntity> IdUsers { get; set; }
    public DbSet<TestGuidEntity> GuidUsers { get; set; }
}