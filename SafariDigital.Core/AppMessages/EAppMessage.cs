namespace SafariDigital.Core.AppMessages;

public enum EAppMessage
{
    UnhandledException = 0,

    // Authentication
    TokenNotKnown = 1000,
    WrongCredentials = 1001,
    TooManyLoginAttempts = 1002,
    UserNotActive = 1003
}