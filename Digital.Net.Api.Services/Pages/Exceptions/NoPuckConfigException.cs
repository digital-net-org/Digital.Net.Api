using Digital.Net.Api.Core.Exceptions;

namespace Digital.Net.Api.Services.Pages.Exceptions;

public class NoPuckConfigException() : DigitalException($"No puck config available. Please upload a config file.");