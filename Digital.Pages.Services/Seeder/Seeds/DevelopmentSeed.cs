using Digital.Pages.Data.Models.Users;

namespace Digital.Pages.Services.Seeder.Seeds;

public static class DevelopmentSeed
{
    public static List<User> Users =>
    [
        new()
        {
            Username = "admin",
            Login = "admin",
            Password = "$2a$10$XvU87xjehCAoA7W64lFDJ.FT/VxO19qEL0eyKnwdYXv3NlFxRVTm6", // Devpassword123!
            Email = "fake-admin@fake.com",
            Role = UserRole.Admin,
            IsActive = true
        },

        new()
        {
            Username = "superAdmin",
            Login = "superAdmin",
            Password = "$2a$10$XvU87xjehCAoA7W64lFDJ.FT/VxO19qEL0eyKnwdYXv3NlFxRVTm6",
            Email = "fake-super-admin@fake.com",
            Role = UserRole.SuperAdmin,
            IsActive = true
        },

        new()
        {
            Username = "user",
            Login = "user",
            Password = "$2a$10$XvU87xjehCAoA7W64lFDJ.FT/VxO19qEL0eyKnwdYXv3NlFxRVTm6",
            Email = "fake-user@fake.com",
            Role = UserRole.User,
            IsActive = true
        }
    ];
}