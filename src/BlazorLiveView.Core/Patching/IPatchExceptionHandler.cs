using Microsoft.Extensions.Logging;

namespace BlazorLiveView.Core.Patching;

internal interface IPatchExceptionHandler
{
    void LogPrefixException<TPatcher>(
        ILogger<TPatcher> logger,
        string methodName,
        Exception ex
    );

    void LogPostfixException<TPatcher>(
        ILogger<TPatcher> logger,
        string methodName,
        Exception ex
    );
}
