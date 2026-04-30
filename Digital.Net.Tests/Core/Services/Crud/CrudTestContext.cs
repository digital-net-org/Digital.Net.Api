using Digital.Net.Core.Entities.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Tests.Core.Services.Crud;

public class CrudTestContext(DbContextOptions<CrudTestContext> options) : DbContext(options)
{
    public DbSet<CrudTestEntity> TestEntities { get; init; }
    public DbSet<CrudTestChild> TestChildren { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.AddInterceptors(new TimestampInterceptor());

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("crud_test");

        builder
            .Entity<CrudTestEntity>()
            .HasMany(e => e.Children)
            .WithOne(c => c.Parent)
            .HasForeignKey(c => c.TestEntityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Entity<CrudTestEntity>()
            .HasOne(e => e.LockedChild)
            .WithMany()
            .HasForeignKey("LockedChildId")
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
