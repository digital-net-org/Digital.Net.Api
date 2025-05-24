using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Api.Core.Random;
using Digital.Net.Api.Entities.Attributes;
using Digital.Net.Api.Entities.Models;

namespace Digital.Net.Api.Controllers.Test.TestUtilities.Context;

public class TestIdEntity : EntityId
{
    [Column("Username"), Required]
    public string Username { get; set; } = Randomizer.GenerateRandomString();

    [Column("Password"), Required, Secret]
    public string Password { get; set; } = Randomizer.GenerateRandomString();

    [Column("Email")]
    public string Email { get; set; } = Randomizer.GenerateRandomString();
}

public class TestGuidEntity : EntityGuid
{
    [Column("Username"), Required]
    public string Username { get; set; } = Randomizer.GenerateRandomString();

    [Column("Password"), Required, Secret]
    public string Password { get; set; } = Randomizer.GenerateRandomString();

    [Column("Email")]
    public string Email { get; set; } = Randomizer.GenerateRandomString();
}