using System.ComponentModel.DataAnnotations;

namespace SafariDigital.Core.Application;

public enum ApplicationMessageCode
{
    [Display(Name = "Unhandled exception")]
    UnhandledException = 0,

    [Display(Name = "Ressource not found")]
    RessourceNotFound = 10,

    [Display(Name = "Payload is incorrect")]
    Incorrect = 11,

    [Display(Name = "Payload or ressource does not meet requirements")]
    DoesNotMeetRequirements = 12,

    [Display(Name = "Invalid format")]
    InvalidFormat = 13,

    [Display(Name = "File size is too heavy")]
    TooHeavy = 14,

    [Display(Name = "Unique constraint violation")]
    UniqueViolation = 15
}