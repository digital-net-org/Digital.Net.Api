using System.ComponentModel.DataAnnotations;

namespace SafariDigital.Core.Application;

public enum EApplicationMessage
{
    UnhandledException = 0,

    // Authentication
    [Display(Name = "Token not known")] TokenNotKnown = 1000,
    [Display(Name = "Wrong credentials")] WrongCredentials = 1001,

    [Display(Name = "Too many login attempts")]
    TooManyLoginAttempts = 1002,

    [Display(Name = "User is not activated")]
    UserNotActive = 1003,

    // User
    [Display(Name = "Password does not meet requirements")]
    PasswordDoesNotMeetRequirements = 1100,

    // Repository
    [Display(Name = "Invalid query argument")]
    QueryError = 2000
}