using Microsoft.Extensions.Logging;

namespace BlazorLiveView.Core.Patching;

internal class PatchExceptionHandler : IPatchExceptionHandler
{
    public void LogPrefixException<TPatcher>(
        ILogger<TPatcher> logger,
        string methodName,
        Exception ex
    )
    {
        logger.LogError(ex, "Prefix patch {MethodName} threw an exception.",
            methodName
        );
    }

    public void LogPostfixException<TPatcher>(
        ILogger<TPatcher> logger,
        string methodName,
        Exception ex
    )
    {
        logger.LogError(ex, "Postfix patch {MethodName} threw an exception.",
            methodName
        );
    }
}
