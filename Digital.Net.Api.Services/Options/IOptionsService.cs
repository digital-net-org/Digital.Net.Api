using Digital.Net.Api.Core.Settings;

namespace Digital.Net.Api.Services.Options;

public interface IOptionsService
{
    public void SettingsInit();
    public T Get<T>(OptionAccessor optionAccessor) where T : notnull;
}