using SafariDigital.Data.Models.Database;

namespace SafariDigital.Data.Models.Seeds;

public static class DevSeed
{
    public static List<string> ApiKeys =>
        ["12345678901234567890123456789012345678901234567890123456789012345678901234567890"];

    public static List<User> Users =>
    [
        new()
        {
            Username = "admin",
            Password = "$2a$10$XvU87xjehCAoA7W64lFDJ.FT/VxO19qEL0eyKnwdYXv3NlFxRVTm6", // Devpassword123!
            Email = "fake-admin@fake.com",
            Role = EUserRole.Admin,
            IsActive = true
        },

        new()
        {
            Username = "superAdmin",
            Password = "$2a$10$XvU87xjehCAoA7W64lFDJ.FT/VxO19qEL0eyKnwdYXv3NlFxRVTm6",
            Email = "fake-super-admin@fake.com",
            Role = EUserRole.SuperAdmin,
            IsActive = true
        },

        new()
        {
            Username = "user",
            Password = "$2a$10$XvU87xjehCAoA7W64lFDJ.FT/VxO19qEL0eyKnwdYXv3NlFxRVTm6",
            Email = "fake-user@fake.com",
            Role = EUserRole.User,
            IsActive = true
        }
    ];
}