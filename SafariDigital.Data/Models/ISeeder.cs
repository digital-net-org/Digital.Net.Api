using Safari.Net.Core.Messages;

namespace SafariDigital.Data.Models;

public interface ISeeder
{
    Task<Result> SeedDevelopmentData();
}