using Digital.Pages.Data.Models.ApiKeys;
using Digital.Pages.Data.Models.ApiTokens;
using Digital.Pages.Data.Models.Avatars;
using Digital.Pages.Data.Models.Documents;
using Digital.Pages.Data.Models.Events;
using Digital.Pages.Data.Models.Frames;
using Digital.Pages.Data.Models.Users;
using Digital.Pages.Data.Models.Views;
using Microsoft.EntityFrameworkCore;

namespace Digital.Pages.Data.Context;

public class DigitalContext(DbContextOptions<DigitalContext> options)
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