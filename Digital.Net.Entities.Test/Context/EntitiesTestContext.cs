using Digital.Net.Entities.Interceptors;
using Digital.Net.Entities.Test.Models;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Entities.Test.Context;

public class EntitiesTestContext(DbContextOptions<EntitiesTestContext> options) : DbContext(options)
{
    public DbSet<TestEntity> TestEntities { get; init; }
    public DbSet<TestChild> TestChildren { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.AddInterceptors(new TimestampInterceptor());

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<TestEntity>()
            .HasMany(e => e.Children)
            .WithOne(c => c.Parent)
            .HasForeignKey(c => c.TestEntityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Entity<TestEntity>()
            .HasOne(e => e.LockedChild)
            .WithMany()
            .HasForeignKey("LockedChildId")
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
