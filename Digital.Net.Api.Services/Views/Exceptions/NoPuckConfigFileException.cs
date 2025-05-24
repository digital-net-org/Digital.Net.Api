using Digital.Net.Api.Core.Exceptions;

namespace Digital.Net.Api.Services.Views.Exceptions;

public class NoPuckConfigFileException(
    int id
) : DigitalException($"Puck config id:{id} does not has any file associated with it.");