namespace CWI.Application.Common.Logging;

public static class ErrorLogConstants
{
    public const string AlreadyLoggedHttpContextItemKey = "CWI.ErrorLogAlreadyWritten";
    public const int MaxRequestBodyLength = 16 * 1024;
}
