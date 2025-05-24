using Digital.Net.Api.Core.Exceptions;

namespace Digital.Net.Api.Services.Views.Exceptions;

public class CannotDeletePublishedConfigException(
    int id
) : DigitalException($"Published puck config with ID {id} cannot be deleted. Please change the published config for each concerned view first.");