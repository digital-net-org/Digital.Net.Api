using Digital.Net.Api.Core.Extensions.ConfigurationUtilities;
using Digital.Net.Api.Core.Extensions.EnumUtilities;
using Digital.Net.Api.Core.Extensions.TypeUtilities;
using Digital.Net.Api.Core.Settings;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.ApplicationOptions;
using Digital.Net.Api.Entities.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Digital.Net.Api.Services.Options;

public class OptionsService(
    ILogger<OptionsService> logger,
    IConfiguration configuration,
    IRepository<ApplicationOption, DigitalContext> appOptionRepository
) : IOptionsService
{
    public void SettingsInit()
    {
        foreach (var (appSettingsAccessor, appOptionAccessor, defaultValue) in ApplicationDefaults.Settings)
        {
            var optionAccessor = appOptionAccessor.GetDisplayName();
            var stored = appOptionRepository
                .Get(o => o.Key == optionAccessor)
                .FirstOrDefault();

            if (stored is not null)
                continue;

            var value = configuration.Get<string>(appSettingsAccessor) ?? defaultValue;
            appOptionRepository.CreateAndSave(
                new ApplicationOption
                {
                    Key = optionAccessor,
                    Value = value
                });

            logger.LogInformation($"Setting {optionAccessor} has been saved in database.");
        }
    }

    public T Get<T>(OptionAccessor optionAccessor) where T : notnull
    {
        var stored = appOptionRepository.Get(o => o.Key == optionAccessor.GetDisplayName());
        if (stored is null)
            throw new InvalidOperationException($"Option {optionAccessor} could not be found");

        return TypeConverter.Convert<T>(stored.First().Value);
    }
}