using Digital.Net.Core.Entities.Interceptors;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Core.Entities.Models.ApiKeys;
using Digital.Net.Core.Entities.Models.ApiTokens;
using Digital.Net.Core.Entities.Models.Avatars;
using Digital.Net.Core.Entities.Models.ConfigValues;
using Digital.Net.Core.Entities.Models.Documents;
using Digital.Net.Core.Entities.Models.Auth;
using Digital.Net.Core.Entities.Models.Mutations;
using Digital.Net.Core.Entities.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Entities.Context;

public class DigitalContext(DbContextOptions<DigitalContext> options) : DbContext(options), ISchemaContext
{
    public const string Schema = "digital_net";
    static string ISchemaContext.Schema => Schema;

    public DbSet<ApiKey> ApiKeys { get; init; }
    public DbSet<ApiToken> ApiTokens { get; init; }
    public DbSet<AuthEvent> AuthEvents { get; init; }
    public DbSet<Avatar> Avatars { get; init; }
    public DbSet<ConfigValue> ConfigValues { get; init; }
    public DbSet<Document> Documents { get; init; }
    public DbSet<EntityMutation> EntityMutations { get; init; }
    public DbSet<User> Users { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => 
        optionsBuilder.AddInterceptors(
            new TimestampInterceptor()
        );

    protected override void OnModelCreating(ModelBuilder builder) =>
        builder
            .HasDefaultSchema(Schema)
            .BuildUser()
            .BuildConfigValue()
            .ConfigurePivots();
}
