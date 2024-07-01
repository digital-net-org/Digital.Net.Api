using Microsoft.EntityFrameworkCore;
using SafariDigital.Database.Models.User;

namespace SafariDigital.Database.Context;

public class SafariDigitalContext(DbContextOptions<SafariDigitalContext> options)
    : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}