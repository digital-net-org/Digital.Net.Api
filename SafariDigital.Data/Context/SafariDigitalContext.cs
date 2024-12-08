using Microsoft.EntityFrameworkCore;
using SafariDigital.Data.Models.Database.ApiKeys;
using SafariDigital.Data.Models.Database.Avatars;
using SafariDigital.Data.Models.Database.Documents;
using SafariDigital.Data.Models.Database.Frames;
using SafariDigital.Data.Models.Database.Records;
using SafariDigital.Data.Models.Database.Users;
using SafariDigital.Data.Models.Database.Views;

namespace SafariDigital.Data.Context;

public class SafariDigitalContext(DbContextOptions<SafariDigitalContext> options)
    : DbContext(options)
{
    public DbSet<ApiKey> ApiKeys { get; init; }
    public DbSet<User> Users { get; init; }
    public DbSet<Document> Documents { get; init; }
    public DbSet<Avatar> Avatars { get; init; }
    public DbSet<RecordedLogin> RecordedLogins { get; init; }
    public DbSet<RecordedToken> RecordedTokens { get; init; }
    public DbSet<View> Views { get; init; }
    public DbSet<Frame> Frames { get; init; }
}
