using Digital.Net.Api.Core.Exceptions;

namespace Digital.Net.Api.Services.Pages.Exceptions;

public class CannotDeletePublishedConfigException(
    int id
) : DigitalException($"Puck config with ID {id} cannot be deleted. Please change the config for each concerned page first.");