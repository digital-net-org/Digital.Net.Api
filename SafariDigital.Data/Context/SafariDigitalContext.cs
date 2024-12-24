using Microsoft.EntityFrameworkCore;
using SafariDigital.Data.Models.ApiKeys;
using SafariDigital.Data.Models.ApiTokens;
using SafariDigital.Data.Models.Avatars;
using SafariDigital.Data.Models.Documents;
using SafariDigital.Data.Models.Events;
using SafariDigital.Data.Models.Frames;
using SafariDigital.Data.Models.Users;
using SafariDigital.Data.Models.Views;

namespace SafariDigital.Data.Context;

public class SafariDigitalContext(DbContextOptions<SafariDigitalContext> options)
    : DbContext(options)
{
    public DbSet<ApiKey> ApiKeys { get; init; }
    public DbSet<ApiToken> ApiTokens { get; init; }
    public DbSet<Avatar> Avatars { get; init; }
    public DbSet<Document> Documents { get; init; }
    public DbSet<EventAuthentication> EventAuthentications { get; init; }
    public DbSet<Frame> Frames { get; init; }
    public DbSet<User> Users { get; init; }
    public DbSet<View> Views { get; init; }
}