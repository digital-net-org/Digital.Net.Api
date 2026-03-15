namespace Digital.Net.Core.Services.Users.Events;

public static class UserEvents
{
    public const string UpdatePassword = "USER_UPDATE_PASSWORD";
    public const string UpdateProfile = "USER_UPDATE_PROFILE";
    public const string CreateApiKey = "USER_CREATE_API_KEY";
    public const string DeleteApiKey = "USER_DELETE_API_KEY";
    public const string CreateUser = "ADMIN_CREATE_USER";
    public const string DeleteUser = "ADMIN_DELETE_USER";
    public const string UpdateUserStatus = "ADMIN_UPDATE_USER_STATUS";
    public const string UpdateUserRole = "ADMIN_UPDATE_USER_ROLE";
}
