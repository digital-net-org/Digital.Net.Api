using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Api.Entities.Models.Events;

public enum EventState
{
    [Display(Name = "Failed")]
    Failed = 0,

    [Display(Name = "Success")]
    Success = 1,

    [Display(Name = "Pending")]
    Pending
}
