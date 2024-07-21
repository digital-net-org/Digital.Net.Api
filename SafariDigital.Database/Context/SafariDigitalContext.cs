using Microsoft.EntityFrameworkCore;
using SafariDigital.Database.Models.DocumentTable;
using SafariDigital.Database.Models.UserTable;

namespace SafariDigital.Database.Context;

public class SafariDigitalContext(DbContextOptions<SafariDigitalContext> options)
    : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Document> Documents { get; set; }
}