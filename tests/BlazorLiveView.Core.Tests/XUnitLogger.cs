using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace BlazorLiveView.Core.Tests;

public sealed class XUnitLogger<T>(ITestOutputHelper output)
    : ILogger<T>, IDisposable
{
    private readonly ITestOutputHelper _output = output;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        _output.WriteLine(state?.ToString());
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull
    {
        return this;
    }

    public void Dispose()
    { }
}